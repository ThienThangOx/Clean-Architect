<template>
  <div class="header-container">
    <div class="header-left">
      <div class="header-left-logo"></div>
      <div class="header-left-company-name">
        <select name="" id="">
          <option value="">
            CÔNG TY TNHH SẢN XUẤT - THƯƠNG MẠI - DỊCH VỤ QUI PHÚC
          </option>
        </select>
      </div>
    </div>
    <div class="header-right">
      <div class="header-right-bell"></div>
      <div class="header-right-avatar"></div>
      <div class="header-right-username">
        <!-- <select name="" id="">
          <option value="">Nguyễn Thiện Thắng</option>
        </select> -->

        <div class="contextmenu-container">
          <div class="contextMenu-username" @click="togleContextMenu()">
            <div>{{ userName }}</div>
            <div id="showContext-menubtn"></div>
          </div>
          <div v-if="isShowMenu" class="contextMenu-option" @click="logOut()">
            {{ $t("EmployeeList.Logout") }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import MResource from "@/components/js/StringResource";
import { apiHandle } from "@/components/js/ApiHandle";
export default {
  name: "TheHeader",
  created() {
    this.userName = this.$store.state.userName;
  },
  data() {
    return {
      isShowMenu: false,
      userName: "",
      revokeTokenApiUrl: "",
    };
  },
  methods: {
    /**
     * togle contextmenu
     * 	created by: Nguyễn Thiện Thắng
     *  created at: 2024/03/26
     */
    togleContextMenu() {
      this.isShowMenu = !this.isShowMenu;
    },
    /**
     * change login satatus and revoke all token
     * 	created by: Nguyễn Thiện Thắng
     *  created at: 2024/03/26
     */
    async logOut() {
      this.revokeTokenApiUrl = `${MResource.apiData.revokeTokenByName}${this.userName}`;
      this.$store.commit("changeAuthenticateStatus", false);
      this.$store.commit("changeUserNameStatus", "");

      const logoutResponse = await apiHandle(
        MResource.apiMethod.post,
        this.revokeTokenApiUrl,
        null,
        this.apiHeaderContentype,
        this.emitter
      );
      if (logoutResponse) {
        localStorage.removeItem(MResource.Token.AccessToken);
        localStorage.removeItem(MResource.Token.RefreshToken);
        localStorage.removeItem(MResource.Token.AccessTokenTime);
        localStorage.removeItem(MResource.Token.RefreshTokenTime);
        localStorage.removeItem("username");
        localStorage.removeItem("locale");
        localStorage.removeItem("language");
        localStorage.removeItem("flag");
        this.$router.push("/login");
      }
    },
  },
};
</script>

<style scoped>
.header-container {
  height: 56px;
  padding: 0 16px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-left,
.header-right {
  height: 100%;
  display: flex;
  align-items: center;
}

.header-left-logo {
  background: url("../../assets/img/Sprites.64af8f61.svg") no-repeat -847px -35px;
  width: 16px;
  height: 14px;
  margin-right: 16px;
}

.header-left-company-name select {
  border: none;
  outline: none;
}

.header-right-bell {
  background: url("../../assets/img/Sprites.64af8f61.svg") no-repeat -788px -30px;
  width: 22px;
  height: 25px;
  margin-right: 24px;
}

.header-right-avatar {
  height: 32px;
  width: 32px;
  background: url("../../assets/img/Sprites.64af8f61.svg") no-repeat -352px -894px;
  border-radius: 50%;
  margin-right: 24px;
}

.contextmenu-container {
  font-size: 14px;
  width: 100px;
  position: relative;
}

.contextMenu-option {
  position: absolute;
  padding: 5px 10px;
  margin-top: 10px;
  width: 100px;
  background-color: #ffff;
  border: 1px solid #e0e0e0;
  border-radius: 4px;
  pointer-events: all;
}

.contextMenu-username {
  display: flex;
  align-items: center;
  justify-content: space-around;
  border-radius: 4px;
}
.contextMenu-username:hover {
  cursor: pointer;
  /* background-color: #faf8f8; */
}

.contextMenu-option:hover {
  cursor: pointer;
  background-color: #faf8f8;
}

#showContext-menubtn {
  background: url("../../assets/img/Sprites.64af8f61.svg") no-repeat -467px -414px;
  width: 10px;
  height: 5px;
}
</style>
