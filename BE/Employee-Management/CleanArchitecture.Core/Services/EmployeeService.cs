﻿using AutoMapper;
using CleanArchitecture.Core.DTOs;
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Exeptions;
using CleanArchitecture.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Services
{
    public class EmployeeService : BaseService<Employee>, IEmployeeService
    {
        IEmployeeRepository _employeeRepository;
        IUnitOfWork _unitOfWork;
        IMapper _mapper;
        ICacheRepository _cacheRepository;
        public EmployeeService(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork, IMapper mapper, ICacheRepository cacheRepository) : base(employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheRepository = cacheRepository;
        }

        public async override Task<ServiceResult> GetAllServiceAsync()
        {
            return await base.GetAllServiceAsync();
        }

        /// <summary>
        /// Get Employee from db by Id and push to service result
        /// </summary>
        /// <param name="id">id to find by</param>
        /// <returns>Serivce result ( sucsess with data or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/9
        public async Task<ServiceResult> GetByIdServiceAsync(Guid id)
        {
            Employee? employee = await _employeeRepository.GetByIdAsync(id);
            // Employee exsist -> return 
            if (employee != null)
            {
                return new ServiceResult()
                {
                    Success = true,
                    Code = System.Net.HttpStatusCode.OK,
                    Data = employee,
                    DevMsg = Resources.MsgResource_VN.GetSuccess,
                    UserMsg = Resources.MsgResource_VN.GetSuccess
                };
            }
            else if (employee == null)
            {
                throw new NotFoundCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.EmployeeNotFound);
            }
            throw new InternalServerErrorCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.FoundErr);
        }

        /// <summary>
        /// insert by repository || Check is all entity' properies is valid before insert
        /// </summary>
        /// <param name="employee">employee to insert </param>
        /// <returns>Service result ( sucsess or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/10
        public async override Task<ServiceResult> InsertServiceAsync(Employee employee)
        {
            // all employee's prop is valid to insert
            if (await BeforeInsertAsync(employee))
            {
                employee.EmployeeId = Guid.NewGuid();
                _unitOfWork.BeginTransaction();
                // insert success
                if (await _unitOfWork.Employee.InsertAsync(employee) > 0)
                {
                    _unitOfWork.Commit();
                    return new ServiceResult()
                    {
                        Success = true,
                        Code = System.Net.HttpStatusCode.Created,
                        Data = 1,
                        DevMsg = Resources.MsgResource_VN.AddSuccess,
                        UserMsg = Resources.MsgResource_VN.AddSuccess
                    };
                }
            }
            throw new InternalServerErrorCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.AddErr);
        }

        /// <summary>
        /// Check is all employee' properies is valid before Update
        /// </summary>
        /// <param name="employee">employee to update </param>
        /// <returns>Service result ( sucsess or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/9
        public async override Task<ServiceResult> UpdateServiceAsync(Employee employee)
        {
            if (await BeforeUpdateAsync(employee))
            {
                _unitOfWork.BeginTransaction();
                // update success
                if (await _unitOfWork.Employee.UpdateAsync(employee) > 0)
                {
                    _unitOfWork.Commit();
                    return new ServiceResult()
                    {
                        Success = true,
                        Code = System.Net.HttpStatusCode.OK,
                        Data = 1,
                        DevMsg = Resources.MsgResource_VN.UpdateSuccess,
                        UserMsg = Resources.MsgResource_VN.UpdateSuccess
                    };
                }
            }
            throw new InternalServerErrorCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.UpdateErr);
        }

        /// <summary>
        /// Check is employee id valid -> delete
        /// </summary>
        /// <param name="employee">employee to delete </param>
        /// <returns>Serivce result ( success or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/9
        public override async Task<ServiceResult> DeleteServiceAsync(Employee employee)
        {
            Employee employeeInDb = await _employeeRepository.GetByIdAsync(employee.EmployeeId);
            //entity exist
            if (employeeInDb != null)
            {
                _unitOfWork.BeginTransaction();
                // delete success
                if (await _employeeRepository.DeleteAsync(employee) > 0)
                {
                    _unitOfWork.Commit();
                    return new ServiceResult()
                    {
                        Success = true,
                        Code = System.Net.HttpStatusCode.OK,
                        Data = 1,
                        DevMsg = Resources.MsgResource_VN.DeleteSucess,
                        UserMsg = Resources.MsgResource_VN.DeleteSucess,
                    };
                }
            }
            else if (employeeInDb == null)
            {
                throw new NotFoundCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.EmployeeNotFound);
            }
            throw new BadRequestCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.DeleteErr);
        }

        #region Before action


        /// <summary>
        /// Check is  Employee's code valid or not before insert 
        /// </summary>
        /// <param name="code">code to check</param>
        /// <returns>
        /// true- code is valid
        /// false- code invalid
        /// </returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/9
        private async Task checkEmployeeCodeValid(string code)
        {
            if (code.ToString().Length > 15)
            {
                throw new BadRequestCustomException(Resources.MsgResource_VN.EployeeCodeLong);
            }
            // employeeCode already exist -> cannot insert
            if (await _employeeRepository.IsEmployeeCodeExistAsync(code))
            {
                throw new BadRequestCustomException($"{Resources.MsgResource_VN.EmpCodeDoNotExistFront}{code}{Resources.MsgResource_VN.EmpCodeDoNotExistBack}");
            }
            string numberPart = code.Substring(code.Length - 6);
            if (!int.TryParse(numberPart, out int currentNumber))
            {
                throw new BadRequestCustomException(Resources.MsgResource_VN.EmployeeCodeRule);
            }
        }

        /// <summary>
        /// Check is all Employee's poroperty valid or not before insert 
        /// </summary>
        /// <param name="employee">Employee to validate</param>
        /// <returns>
        /// true- All property is valid
        /// false- One or more property is not valid
        /// </returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/9
        public async override Task<bool> BeforeInsertAsync(Employee employee)
        {
            await checkEmployeeCodeValid(employee.EmployeeCode);
            // employee name null-> cannot insert
            if (String.IsNullOrEmpty(employee.EmployeeName))
            {
                throw new BadRequestCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.EmployeeNameCannotBeNull);
            }
            return true;
        }

        /// <summary>
        /// Action before update entity into database
        /// </summary>
        /// <param name="employee">Entity to manipulate</param>
        /// <returns>
        /// true-entity valid to update
        /// false- entity valid not to update
        /// </returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/9
        public async override Task<bool> BeforeUpdateAsync(Employee employee)
        {
            Employee employeeInDb = await _employeeRepository.GetByIdAsync(employee.EmployeeId);
            // entity doesn't exist
            if (employeeInDb == null)
            {
                throw new NotFoundCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.EmployeeNotFound);
            }
            // Code exit -> Check to see if this Code belongs to the employee who is editing 
            // true -> update | false -> throw duplicate Code exeption
            if (await _employeeRepository.IsEmployeeCodeExistAsync(employee.EmployeeCode))
            {
                string numberPart = employee.EmployeeCode.Substring(employee.EmployeeCode.Length - 6);
                if (!int.TryParse(numberPart, out int currentNumber))
                {
                    throw new BadRequestCustomException(Resources.MsgResource_VN.EmployeeCodeRule);
                }
                if (employee.EmployeeCode != employeeInDb.EmployeeCode)
                {
                    throw new BadRequestCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.EmployeeCodeDuplicate);
                }
            }
            return true;
        }
        #endregion

        /// <summary>
        /// Calculate to get bigger employeeCode
        /// </summary>
        /// <returns>new Employee Code/returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/21
        public async Task<ServiceResult> CalEmployeeCodeAsync()
        {
            string currentCode = await _employeeRepository.GetBiggestEmployeeCodeAsync();
            // no record in db
            if (currentCode == null)
            {
                return new ServiceResult()
                {
                    Success = true,
                    Code = HttpStatusCode.OK,
                    Data = "NV-000000",
                    DevMsg = Resources.MsgResource_VN.GetSuccess,
                    UserMsg = Resources.MsgResource_VN.GetSuccess
                };
            }
            //get last six characters
            string numberPart = currentCode.Substring(currentCode.Length - 6);
            // get prefix
            string prefix = currentCode.Substring(0, currentCode.Length - numberPart.Length);
            currentCode.PadLeft(6, '0');
            if (int.TryParse(numberPart, out int currentNumber))
            {
                // increase int number's value
                currentNumber++;
                // Format
                string newEmployeeCode = $"{prefix}{currentNumber}";
                return new ServiceResult()
                {
                    Success = true,
                    Code = HttpStatusCode.OK,
                    Data = newEmployeeCode,
                    DevMsg = Resources.MsgResource_VN.GetSuccess,
                    UserMsg = Resources.MsgResource_VN.GetSuccess
                };
            }
            throw new InternalServerErrorCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.CommonErrr);
        }

        /// <summary>
        /// Paging records base page, pageSize and search key
        /// </summary>
        /// <param name="page">Current page </param>
        ///  <param name="pageSize">record'number /page </param>
        ///  <param name="key">search key </param>
        /// <returns>Service result ( sucsess or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/15/1
        public async override Task<ServiceResult> PagingServiceAsync(int page, int pageSize, string key)
        {
            // page have to > 0
            if (page < 1)
            {
                throw new BadRequestCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.PagingErr);
            }
            List<Employee> pagingList = await _employeeRepository.PagingAsync((page - 1) * pageSize, pageSize, key);
            if (pagingList.Count >= 0)
            {

                Page<Employee> pageObject = new Page<Employee>()
                {
                    ListRecord = pagingList,
                    CurrentPage = page,
                    TotalPage = await _employeeRepository.GetPageSizeAsync<Employee>(pageSize, key)
                };
                return new ServiceResult()
                {

                    Data = new
                    {
                        records = pageObject,
                        totalRecords = await _employeeRepository.CountTotalRecordByKey<Employee>(key)
                    },
                    Success = true,
                    Code = System.Net.HttpStatusCode.OK,
                    DevMsg = Resources.MsgResource_VN.GetSuccess,
                    UserMsg = Resources.MsgResource_VN.GetSuccess
                };
            }
            throw new InternalServerErrorCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.GetErr);
        }

        /// <summary>
        /// Delete employess by EmployeeId list
        /// </summary>
        /// <param name="employeesId">Employee id list to delete</param>
        /// <returns>Serivce result ( sucsess with data or failed with all details )</returns>	
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/9
        public async Task<ServiceResult> MultipleDeleteAsync(List<Guid?> employeesId)
        {
            // empty list
            if (employeesId.Count == 0 || employeesId == null)
            {
                throw new BadRequestCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.EmptyEmployeeList);
            }
            int countDelete = 0;
            _unitOfWork.BeginTransaction();
            // loop form the first employee to the last employee, get Id and delete
            foreach (Guid? id in employeesId)
            {
                // employee exist -> can be delete
                var employee = await _unitOfWork.Employee.GetByIdAsync(id);
                if (employee != null)
                {
                    // delete success
                    if (await _unitOfWork.Employee.DeleteAsync(employee) <= 0)
                    {
                        _unitOfWork.RollBack();
                        throw new InternalServerErrorCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.DeleteErr);
                    }
                    countDelete++;
                }
                else
                {
                    throw new NotFoundCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.EmployeeNotFound);
                }
            }
            // commit delete
            _unitOfWork.Commit();
            return new ServiceResult()
            {
                Success = true,
                Code = System.Net.HttpStatusCode.OK,
                Data = countDelete,
                UserMsg = Resources.MsgResource_VN.DeleteSucess
            };
        }

        #region FIle Action
        /// <summary>
        /// exprot file for user base user's current page, page size and search key word
        /// </summary>
        /// <param name="page">Current page </param>
        ///  <param name="pageSize">record'number /page </param>
        ///  <param name="key">search key </param>
        ///  <param name="importKey">key to import if data mode = 2 </param>
        ///  <param name="dataMode">data resource
        ///  1 - From DataBase
        ///  2 - Form cache (EmployeeImport)
        /// <returns>Service result ( sucsess or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/15/1
        //public async Task<ServiceResult> ExportFileAsync(int page, int pageSize, string key, string importKey, int dataMode)
        //{
        //    try
        //    {
        //        List<EmployeeImport> employees = new List<EmployeeImport>();
        //        // data from data base
        //        if (dataMode == 1)
        //        {
        //            List<Employee> employeesInDb = await _employeeRepository.PagingAsync((page - 1) * pageSize, pageSize, key);
        //            employees = _mapper.Map<List<Employee>, List<EmployeeImport>>(employeesInDb);
        //        }
        //        else
        //        {
        //            employees = _cacheRepository.GetCache<List<EmployeeImport>>(importKey);
        //        }


        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //        // create an excel file
        //        using (ExcelPackage package = new ExcelPackage())
        //        {
        //            // create a work sheet
        //            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(Resources.CommonRs.EmployeeWorkSheetName);
        //            int row = 4;
        //            int index = 1;
        //            ExcelRange titleRange = worksheet.Cells["A1:I1"];
        //            worksheet.Cells["A2:I2"].Merge = true;
        //            titleRange.Merge = true;
        //            titleRange.Value = Resources.CommonRs.EmployeeFileHeader;
        //            titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        //            using (ExcelRange rng = worksheet.Cells["A1:I1"])
        //            {
        //                rng.Style.Font.Bold = true;
        //                rng.Style.Font.Size = 14;
        //            }

        //            // header title
        //            worksheet.Cells[3, 1].Value = "STT";
        //            worksheet.Cells[3, 2].Value = "Mã nhân viên";
        //            worksheet.Cells[3, 3].Value = "Tên nhân viên";
        //            worksheet.Cells[3, 4].Value = "Giới tính";
        //            worksheet.Cells[3, 5].Value = "Ngày sinh";
        //            worksheet.Cells[3, 6].Value = "Chức danh";
        //            worksheet.Cells[3, 7].Value = "Tên đơn vị";
        //            worksheet.Cells[3, 8].Value = "Số tài khoản";
        //            worksheet.Cells[3, 9].Value = "Tên ngân hàng";
        //            for (int col = 1; col <= 9; col++)
        //            {
        //                worksheet.Cells[3, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //                worksheet.Cells[3, col].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
        //            }

        //            // write data
        //            foreach (var employee in employees)
        //            {
        //                worksheet.Cells[row, 1].Value = index;
        //                worksheet.Cells[row, 2].Value = employee.EmployeeCode;
        //                worksheet.Cells[row, 3].Value = employee.EmployeeName;
        //                string gender;
        //                switch (employee.Gender)
        //                {
        //                    case 0:
        //                        gender = "Nam";
        //                        break;
        //                    case 1:
        //                        gender = "Nữ";
        //                        break;
        //                    default:
        //                        gender = "Khác";
        //                        break;
        //                }
        //                worksheet.Cells[row, 4].Value = gender;
        //                worksheet.Cells[row, 5].Style.Numberformat.Format = "dd/MM/yyyy";
        //                worksheet.Cells[row, 5].Value = employee.BirthDate;
        //                worksheet.Cells[row, 6].Value = employee.PositionName;
        //                worksheet.Cells[row, 7].Value = employee.DepartmentName;
        //                worksheet.Cells[row, 8].Value = employee.BankAccount;
        //                worksheet.Cells[row, 9].Value = employee.BankName;
        //                // auto fit column's with
        //                worksheet.Cells.AutoFitColumns();
        //                if (dataMode == 2)
        //                {
        //                    worksheet.Cells[3, 10].Value = "Trạng thái";
        //                    worksheet.Column(10).Width = 50;
        //                    worksheet.Cells[3, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //                    worksheet.Cells[3, 10].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

        //                    if (employee.ErrorList.Count > 0)
        //                    {
        //                        var indentedErrors = employee.ErrorList.Select(error => "\t" + error);
        //                        worksheet.Cells[row, 10].Style.WrapText = true;
        //                        worksheet.Cells[row, 10].Value = string.Join(Environment.NewLine, indentedErrors);
        //                        worksheet.Cells[row, 10].Style.Font.Color.SetColor(System.Drawing.Color.Red);
        //                        worksheet.Cells[row, 10].AddComment("Errors occurred", "Author");
        //                    }
        //                    else
        //                    {
        //                        worksheet.Cells[row, 10].Value = Resources.MsgResource_VN.Valid;
        //                    }
        //                }
        //                row++;
        //                index++;
        //            }
        //            worksheet.Rows[4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //            MemoryStream stream = new MemoryStream(package.GetAsByteArray());
        //            byte[] fileBytes = stream.ToArray();
        //            string fileName = Resources.CommonRs.EmployeeFileName;
        //            var excelData = new Dictionary<string, object>
        //    {
        //        { "FileBytes", fileBytes },
        //        { "FileName", fileName }
        //    };

        //            return await Task.FromResult(new ServiceResult
        //            {
        //                Success = true,
        //                Code = HttpStatusCode.OK,
        //                Data = excelData,
        //                DevMsg = Resources.MsgResource_VN.UpLoadFailed,
        //                UserMsg = Resources.MsgResource_VN.UpLoadFailed
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new BadRequestCustomException(ex.Message);
        //    }
        //}


        public async Task<ServiceResult> ExportFileAsync(int page, int pageSize, string key, string importKey, int dataMode)
        {
            try
            {
                List<EmployeeImport> employees = new List<EmployeeImport>();
                // data from data base
                if (dataMode == 1)
                {
                    List<Employee> employeesInDb = await _employeeRepository.PagingAsync((page - 1) * pageSize, pageSize, key);
                    employees = _mapper.Map<List<Employee>, List<EmployeeImport>>(employeesInDb);
                }
                else
                {
                    employees = _cacheRepository.GetCache<List<EmployeeImport>>(importKey);
                }


                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // create an excel file
                using (ExcelPackage package = new ExcelPackage())
                {
                    // create a work sheet
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(Resources.CommonRs.EmployeeWorkSheetName);
                    int row = 4;
                    int index = 1;
                    int colCount = 9; // Số lượng cột mặc định
                    int dataStartRow = 4; // Dòng bắt đầu ghi dữ liệu
                    ExcelRange titleRange = worksheet.Cells["A1:I1"];
                    worksheet.Cells["A2:I2"].Merge = true;
                    titleRange.Merge = true;
                    titleRange.Value = Resources.CommonRs.EmployeeFileHeader;
                    titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    using (ExcelRange rng = worksheet.Cells["A1:I1"])
                    {
                        rng.Style.Font.Bold = true;
                        rng.Style.Font.Size = 14;
                    }

                    // Header title
                    string[] headerTitles = { "STT", "Mã nhân viên", "Tên nhân viên", "Giới tính", "Ngày sinh", "Chức danh", "Tên đơn vị", "Số tài khoản", "Tên ngân hàng" };
                    for (int col = 1; col <= colCount; col++)
                    {
                        worksheet.Cells[3, col].Value = headerTitles[col - 1];
                        worksheet.Cells[3, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[3, col].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    // Write data
                    foreach (var employee in employees)
                    {
                        // Ghi dữ liệu vào từng ô tương ứng
                        worksheet.Cells[row, 1].Value = index;
                        worksheet.Cells[row, 2].Value = employee.EmployeeCode;
                        worksheet.Cells[row, 3].Value = employee.EmployeeName;
                        string gender;
                        switch (employee.Gender)
                        {
                            case 0:
                                gender = "Nam";
                                break;
                            case 1:
                                gender = "Nữ";
                                break;
                            default:
                                gender = "Khác";
                                break;
                        }
                        worksheet.Cells[row, 4].Value = gender;
                        worksheet.Cells[row, 5].Style.Numberformat.Format = "dd/MM/yyyy";
                        worksheet.Cells[row, 5].Value = employee.BirthDate;
                        worksheet.Cells[row, 6].Value = employee.PositionName;
                        worksheet.Cells[row, 7].Value = employee.DepartmentName;
                        worksheet.Cells[row, 8].Value = employee.BankAccount;
                        worksheet.Cells[row, 9].Value = employee.BankName;

                        // Tự động điều chỉnh độ rộng của cột
                        worksheet.Cells.AutoFitColumns();

                        if (dataMode == 2)
                        {
                            int statusColumnIndex = colCount + 1; // Cột trạng thái
                            worksheet.Cells[3, statusColumnIndex].Value = "Trạng thái";
                            worksheet.Column(statusColumnIndex).Width = 50;
                            worksheet.Cells[3, statusColumnIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[3, statusColumnIndex].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                            if (employee.ErrorList.Count > 0)
                            {
                                var indentedErrors = employee.ErrorList.Select(error => "\t" + error);
                                worksheet.Cells[row, statusColumnIndex].Style.WrapText = true;
                                worksheet.Cells[row, statusColumnIndex].Value = string.Join(Environment.NewLine, indentedErrors);
                                worksheet.Cells[row, statusColumnIndex].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                                worksheet.Cells[row, statusColumnIndex].AddComment(Resources.MsgResource_VN.FixErrorToImport, "Author");
                            }
                            else
                            {
                                worksheet.Cells[row, statusColumnIndex].Value = Resources.MsgResource_VN.Valid;
                            }
                        }
                        row++;
                        index++;
                    }

                    // Tự động can giữa dòng dữ liệu
                    worksheet.Cells[dataStartRow, 1, row - 1, colCount].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    MemoryStream stream = new MemoryStream(package.GetAsByteArray());
                    byte[] fileBytes = stream.ToArray();
                    string fileName = Resources.CommonRs.EmployeeFileName;
                    var excelData = new Dictionary<string, object>
            {
                { "FileBytes", fileBytes },
                { "FileName", fileName }
            };

                    return await Task.FromResult(new ServiceResult
                    {
                        Success = true,
                        Code = HttpStatusCode.OK,
                        Data = excelData,
                        DevMsg = Resources.MsgResource_VN.UpLoadFailed,
                        UserMsg = Resources.MsgResource_VN.UpLoadFailed
                    });
                }
            }
            catch (Exception ex)
            {
                throw new BadRequestCustomException(ex.Message);
            }
        }
        /// <summary>
        /// recevie a file and save to db
        /// </summary>
        /// <param name="file">file to save to db</param>
        /// <returns>S( sucsess with data or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/9
        public async Task<ServiceResult> PreviewFileAsync(IFormFile excelFile)
        {
            if (excelFile == null)
            {
                throw new BadRequestCustomException(Resources.MsgResource_VN.FileNotValid);
            }

            // Check file extension
            string fileName = excelFile.FileName;
            if (Path.GetExtension(fileName) != ".xlsx")
            {
                throw new BadRequestCustomException(Resources.MsgResource_VN.EmployeeFile_FileWrongFormat);
            }

            var filePath = Path.Combine(Resources.CommonRs.UploadFolderName, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await excelFile.CopyToAsync(stream);
            }
            // file exist
            if (File.Exists(filePath))
            {
                List<EmployeeImport> employeesImport = new();
                List<Employee> validEmployees = new();
                List<EmployeeImport> invalidEmployees = new();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    string[] expectedHeaders = { "STT", "Mã nhân viên", "Tên nhân viên", "Giới tính", "Ngày sinh", "Chức danh", "Tên đơn vị", "Số tài khoản", "Tên ngân hàng" };
                    // check headers title are match with Demo file or not
                    for (int i = 0; i < expectedHeaders.Length; i++)
                    {
                        // not match -> throw exption
                        if (worksheet.Cells[3, i + 1].Value?.ToString()?.Trim() != expectedHeaders[i])
                        {
                            throw new BadRequestCustomException(Resources.MsgResource_VN.EmployeeFile_FileNotValid);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(worksheet.Cells[3, 10].Value?.ToString()))
                    {
                        throw new BadRequestCustomException(Resources.MsgResource_VN.EmployeeFile_FileNotValid);
                    }
                    for (int row = 4; row <= rowCount; row++)
                    {
                        EmployeeImport employeeImport = new EmployeeImport();
                        employeeImport.EmployeeCode = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                        employeeImport.EmployeeName = worksheet.Cells[row, 3].Value?.ToString()?.Trim();

                        switch (worksheet.Cells[row, 4].Value?.ToString()?.Trim().ToLower())
                        {
                            case "nam":
                                employeeImport.Gender = 0;
                                break;
                            case "nữ":
                                employeeImport.Gender = 1;
                                break;
                            case "khác":
                                employeeImport.Gender = 2;
                                break;
                            default:
                                employeeImport.Gender = null;
                                break;
                        }

                        employeeImport.PositionName = worksheet.Cells[row, 6].Value?.ToString()?.Trim();
                        employeeImport.DepartmentName = worksheet.Cells[row, 7].Value?.ToString()?.Trim();
                        employeeImport.BankAccount = worksheet.Cells[row, 8].Value?.ToString()?.Trim();
                        employeeImport.BankName = worksheet.Cells[row, 9].Value?.ToString()?.Trim();
                        string dateString = worksheet.Cells[row, 5].Value?.ToString()?.Trim();
                        if (!String.IsNullOrEmpty(dateString))
                        {
                            // Chuyển "SA" thành "AM" hoặc "PM"
                            dateString = dateString.Replace("SA", "AM").Replace("CH", "PM"); // CH có thể là viết tắt của "CHIỀU" hoặc "CHIỀU HAY"
                            DateTime dateTime;
                            string[] formats = { "dd/MM/yyyy h:mm:ss tt", "dd/MM/yyyy H:mm:ss", "dd/MM/yyyy" }; // Thêm "d/M/yyyy" vào danh sách định dạng
                            if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                            {
                                Console.WriteLine(dateTime);
                                employeeImport.BirthDate = dateTime;
                            }
                            else
                            {
                                employeeImport.ErrorList.Add(Resources.MsgResource_VN.DateWrongFormat);
                            }
                        }

                        //employyee name is required
                        if (string.IsNullOrEmpty(employeeImport.EmployeeName))
                        {
                            employeeImport.ErrorList.Add(Resources.MsgResource_VN.EmployeeFIle_NameIsRequired);
                        }
                        // employee Code is required
                        if (string.IsNullOrEmpty(employeeImport.EmployeeCode))
                        {
                            employeeImport.ErrorList.Add(Resources.MsgResource_VN.EmployeeFile_EmployeeCodeCanNotBeNull);
                        }
                        // employeecode duplicate -> err
                        else if (await _unitOfWork.Employee.IsEmployeeCodeExistAsync(employeeImport.EmployeeCode))
                        {
                            employeeImport.ErrorList.Add($"- {Resources.MsgResource_VN.EmpCodeDoNotExistFront}" +
                                                        $"{employeeImport.EmployeeCode}" +
                                                        $"{Resources.MsgResource_VN.EmpFileCodeDoNotExistBack}");
                        }
                        // employee code wrong format
                        else
                        {
                            if (employeeImport.EmployeeCode.ToString().Length > 15)
                            {
                                employeeImport.ErrorList.Add($"- {Resources.MsgResource_VN.EployeeCodeLong}");
                            }
                            string numberPart = (employeeImport.EmployeeCode.Substring(employeeImport.EmployeeCode.Length - 6));
                            if (!int.TryParse(numberPart, out int currentNumber))
                            {
                                employeeImport.ErrorList.Add(Resources.MsgResource_VN.EmployeeCodeRule);
                            }
                        }

                        // department name is required
                        if (string.IsNullOrEmpty(employeeImport.DepartmentName))
                        {
                            employeeImport.ErrorList.Add(Resources.MsgResource_VN.EmployeeFile_DepartmentNameIsRequired);
                        }
                        else
                        {
                            Department department = await _unitOfWork.Department.GetByDepartmentNameAsync(employeeImport.DepartmentName);
                            // check is department is exist or not
                            if (department != null)
                            {
                                employeeImport.DepartmentId = department.DepartmentId;
                            }
                            else
                            {
                                employeeImport.ErrorList.Add(Resources.MsgResource_VN.EmployeeFile_DepartmentNotValid);
                            }
                        }
                        employeeImport.IsEmployeeValid = employeeImport.ErrorList.Count == 0;
                        // employee valid 
                        if ((bool)employeeImport.IsEmployeeValid)
                        {
                            Employee employee = _mapper.Map<Employee>(employeeImport);
                            validEmployees.Add(employee);
                        }
                        else
                        {
                            invalidEmployees.Add(employeeImport);
                        }
                        employeesImport.Add(employeeImport);
                    }
                    TimeSpan timeSpan = TimeSpan.FromMinutes(30);
                    // key for valid list
                    var validToImportCacheKey = Guid.NewGuid().ToString();
                    // key for error list
                    var invalidEmployeeCacheKey = Guid.NewGuid().ToString();
                    // key for both
                    var employeeImportCacheKey = Guid.NewGuid().ToString();
                    _cacheRepository.SetCache<List<EmployeeImport>>(invalidEmployeeCacheKey, invalidEmployees, timeSpan);
                    _cacheRepository.SetCache<List<EmployeeImport>>(employeeImportCacheKey, employeesImport, timeSpan);
                    _cacheRepository.SetCache<List<Employee>>(validToImportCacheKey, validEmployees, timeSpan);
                    return new ServiceResult()
                    {
                        Success = true,
                        Code = System.Net.HttpStatusCode.OK,
                        Data = new
                        {
                            employeesImport = employeesImport,
                            validToImportCacheKey = validToImportCacheKey,
                            invalidEmployeeCacheKey = invalidEmployeeCacheKey,
                            employeeImportCacheKey = employeeImportCacheKey,
                        },
                        DevMsg = Resources.MsgResource_VN.UploadSuccess,
                        UserMsg = Resources.MsgResource_VN.UploadSuccess,
                    };
                }
            }
            // updload failed
            else
            {
                throw new BadRequestCustomException(Resources.MsgResource_VN.UpLoadFailed);
            }
        }

        /// <summary>
        /// insert all valid employee base cache key from previous preview
        /// </summary>
        ///  <param name="key">Cache key to get data to insert employee into db </param>
        /// <returns>Service result ( sucsess or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/2/1
        public async Task<ServiceResult> ImportFileAsync(string key)
        {
            // cache not exist
            if (_cacheRepository.IsCacheExist(key))
            {
                List<Employee> employeeList = _cacheRepository.GetCache<List<Employee>>(key);
                _unitOfWork.BeginTransaction();
                // insert all valid employee
                foreach (Employee employee in employeeList)
                {
                    employee.EmployeeId = Guid.NewGuid();
                    if (await _unitOfWork.Employee.InsertAsync(employee) <= 0)
                    {
                        _unitOfWork.RollBack();
                        throw new BadRequestCustomException(Resources.MsgResource_VN.CommonErrr);
                    }
                }
                _unitOfWork.Commit();
                return new ServiceResult()
                {
                    Success = true,
                    Code = System.Net.HttpStatusCode.Created,
                    Data = employeeList.Count,
                    DevMsg = Resources.MsgResource_VN.AddSuccess,
                    UserMsg = Resources.MsgResource_VN.AddSuccess,
                };
            }
            throw new InternalServerErrorCustomException(Resources.MsgResource_VN.CommonErrr);
        }
        #endregion

        /// <summary>
        /// Get total employee record in DB
        /// </summary>
        /// <returns>Serivce result ( sucsess with data or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/1/9
        public async Task<ServiceResult> GetTotalRecordAsync()
        {
            var totalRecord = (await _unitOfWork.Employee.GetAllAsync()).Count;
            if (totalRecord >= 0)
            {
                return new ServiceResult()
                {
                    Success = true,
                    Code = System.Net.HttpStatusCode.OK,
                    Data = totalRecord,
                    UserMsg = Resources.MsgResource_VN.GetSuccess,
                    DevMsg = Resources.MsgResource_VN.GetSuccess,
                };
            }
            throw new InternalServerErrorCustomException(Resources.MsgResource_VN.GetErr);
        }

        /// <summary>
        /// Get sample excelfile to import employee
        /// </summary>
        /// <returns>Service result ( sucsess or failed with all details )</returns>
        ///  created by: Nguyễn Thiện Thắng
        ///  created at: 2024/3/20
        public async Task<ServiceResult> GetSampleExcelFileAsync()
        {
            var filePath = Path.Combine(Resources.CommonRs.SampleFolder, Core.Resources.CommonRs.EmployeeSampleFileName);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // file exist
            if (File.Exists(filePath))
            {
                var package = new ExcelPackage(new FileInfo(filePath));
                MemoryStream stream = new MemoryStream(package.GetAsByteArray());
                byte[] fileBytes = stream.ToArray();
                var fileName = Core.Resources.CommonRs.EmployeeSampleFileName;
                var excelData = new Dictionary<string, object>
                {
                    { "FileBytes", fileBytes },
                    { "FileName", fileName }
                };
                return await Task.FromResult(new ServiceResult
                {
                    Success = true,
                    Code = HttpStatusCode.OK,
                    Data = excelData,
                    DevMsg = Resources.MsgResource_VN.UpLoadFailed,
                    UserMsg = Resources.MsgResource_VN.UpLoadFailed
                });
            }
            // errr
            throw new InternalServerErrorCustomException(CleanArchitecture.Core.Resources.MsgResource_VN.CommonErrr);
        }
    }
}

