import axios from "axios";
import MResource from "../js/StringResource"
import router from "./Router";
import { store } from "@/main";
import { handleErr } from "./ApiHandle";

/**
    * get new access tokent and refresh token base expiration time 
    * @param {*} emitter - to operate with error message
    * created by: Nguyễn Thiện Thắng
    * created at: 2024/2/26
    */
export async function getNewToken(emitter) {
    var now = new Date();
    var expirationRefreshTokenTimeString = localStorage.getItem("expirationRefreshToken");
    var expirationRefreshTokenTime = new Date(expirationRefreshTokenTimeString);

    var expirationAccessTokenTimeString = localStorage.getItem("expirationToken");
    var expirationAccessTokenTime = new Date(expirationAccessTokenTimeString);

    // The login session expires -> forcing the user to log in again
    if (expirationRefreshTokenTime <= now) {
        store.commit('changeAuthenticateStatus', false);
        router.push("/login");
        emitter.emit(
            MResource.Event.TogleDialog,
            [MResource.DialogMsg.expiresLoginSession],
            MResource.DialogType.warning,
            MResource.DialogIcon.warning,
        );
        // The AccessToken expires -> base current accestoken and current refresh token to get new access token
    } else if (expirationAccessTokenTime <= now) {
        let data = {
            "AccessToken": localStorage.getItem(MResource.Token.AccessToken),
            "RefreshToken": localStorage.getItem(MResource.Token.RefreshToken),
        };
        const apiResponse = await axios.post(
            MResource.apiData.getNewAccessToken,
            data,
            {
                headers: {
                    "Content-Type": MResource.apiHeaderContentType.applicationType,
                },
            }
        );
        emitter.emit(MResource.Event.TogleLoading, false);
        if (!apiResponse.data.Success) {
            handleErr(apiResponse, MResource.ErrorType.misa, emitter);
        } else {
            localStorage.setItem(MResource.Token.AccessToken, apiResponse.data.Data.accessToken);
            localStorage.setItem(MResource.Token.RefreshToken, apiResponse.data.Data.refreshToken);
            localStorage.setItem(
                "expirationToken",
                apiResponse.data.Data.Expiration
            );
            localStorage.setItem(
                "expirationRefreshToken",
                apiResponse.data.Data.ExpirationRefreshToken
            );

        }
    }
}

/**
    * check user is authentication before render any component
    * @param {*} emitter - to operate with error message
    * created by: Nguyễn Thiện Thắng
    * created at: 2024/2/26
    */
export async function checkAuthentication(emitter) {
    // if (!store.state.isAuthenticate) {
    //     console.log("h");
    //     router.push("/login");
    // }
    var now = new Date();
    var expirationTimeString = localStorage.getItem("expirationToken");
    var expirationTime = new Date(expirationTimeString);
    if (store.state.isAuthenticate && expirationTime <= now) {
        await getNewToken(emitter);
    }
}