<!-- eslint-disable no-debugger -->
<!-- eslint-disable no-debugger -->
<!-- eslint-disable no-debugger -->
<template>
  <div class="login-container">
    <div class="languague-container">
      <div class="current-language-container" @click="togleLanguageContainer()">
        <div class="curent-language">
          <div :class="flagClass"></div>
          <div>{{ currentLanguague }}</div>
        </div>
      </div>

      <div class="option-laguage-container" v-if="isShowLanguaContainer">
        <div class="option-laguage" @click="changeLanguage('VI')">
          <div class="language-vi-flag-option"></div>
          <div class="laguague-vi-country-option">Tiếng Việt</div>
        </div>

        <div class="option-laguage" @click="changeLanguage('EN')">
          <div class="language-en-flag-option"></div>
          <div class="laguague-eng-country-option">English</div>
        </div>
      </div>
    </div>

    <div class="login-form">
      <div class="form">
        <div class="form-logo">
          <div class="logo"></div>
        </div>
        <div class="form-username">
          <m-text-field
            :isRequired="true"
            :requiredMsg="$t('Login.Error.UsernameIsRequied')"
            ref="usernameRef"
            @update:value="updateValue('Username', $event)"
            :maxInput="true"
            :placeholder="$t('Login.PlaceHolder.PhoneOrEmail')"
          />
        </div>
        <div class="form-password">
          <m-text-filed-hidden
            :isRequired="true"
            :requiredMsg="$t('Login.Error.PasswordIsRequired')"
            ref="passwordRef"
            @update:value="updateValue('Password', $event)"
            :maxInput="true"
            :placeholder="$t('Login.PlaceHolder.PassWord')"
          />
        </div>
        <div class="errorMsg" v-if="!isAuthenticate">
          {{ $t("Login.Error.WrongPasswordOrUsername") }}
        </div>
        <div class="form-forgot-password">
          <a href="#">{{ $t("Login.ForgotPassWord") }}</a>
        </div>
        <div class="form-login-btn">
          <m-button :text="$t('Login.Login')" type="normal" @click="login()" />
        </div>
        <div class="sso-title">
          <div class="sso-title-line"></div>
          <div class="sso-title-text">{{ $t("Login.OrLoginWith") }}</div>
          <div class="sso-title-line"></div>
        </div>

        <div class="sso-method-list">
          <div class="form-google-login sso-method-item"></div>
          <div class="form-apple-login sso-method-item"></div>
          <div class="form-microsoft-login sso-method-item"></div>
        </div>
      </div>
      <div class="form-copy-right">Copyright © 2012 - 2024 MISA JSC</div>
    </div>
  </div>
</template>

<script>
import MButton from "../base/MButton.vue";
import MTextField from "../base/MTextField.vue";
import MTextFiledHidden from "../base/MTextFiledHidden.vue";

import { apiHandle } from "../js/ApiHandle";
import MResource from "../js/StringResource";

export default {
  name: "TheLogin",
  components: {
    MTextField,
    MButton,
    MTextFiledHidden,
  },

  data() {
    return {
      User: {
        Username: "",
        Password: "",
      },
      isAuthenticate: true,
      isShowLanguaContainer: false,
      currentLanguague: this.$store.state.currentLanguague,
      flagClass: this.$store.state.currentFlag,
    };
  },
  methods: {
    /**
     * if all data not empty -> send login request to api
     * 	created by: Nguyễn Thiện Thắng
     *  created at: 2024/1/20
     */
    async login() {
      // eslint-disable-next-line no-debugger
      if (this.isAllDataValid()) {
        const userResponse = await apiHandle(
          MResource.apiMethod.post,
          this.resource.apiData.login,
          {
            Username: this.User.Username,
            Password: this.User.Password,
          },
          MResource.apiHeaderContentType.applicationType,
          this.emitter,
          null
        );
        this.isAuthenticate = this.$store.state.isAuthenticate;
        if (userResponse) {
          this.$store.commit("changeAuthenticateStatus", true);
          this.$store.commit("changeUserNameStatus", this.User.Username);
          localStorage.setItem("username", this.User.Username);
          this.$router.push("/employee-layout");
          localStorage.setItem(
            MResource.Token.AccessToken,
            userResponse.data.Data.Token
          );
          localStorage.setItem(
            MResource.Token.RefreshToken,
            userResponse.data.Data.RefreshToken
          );
          localStorage.setItem(
            MResource.Token.AccessTokenTime,
            userResponse.data.Data.Expiration
          );
          localStorage.setItem(
            MResource.Token.RefreshTokenTime,
            userResponse.data.Data.ExpirationRefreshToken
          );
        }
      }
    },
    /**
     * change language
     * @param {*} laguage language want to change
     * 	created by: Nguyễn Thiện Thắng
     *  created at: 2024/02/26
     */
    changeLanguage(laguage) {
      switch (laguage) {
        case MResource.language.VI:
          this.currentLanguague = "Tiếng Việt";
          this.flagClass = "vn-flag";
          this.$store.commit("changeLanguage", MResource.language.VI);
          this.$store.commit("changeCurrentLanguage", "Tiếng Việt");
          this.$store.commit("changeCurrentFlag", "vn-flag");

          localStorage.setItem("locale", "VI");
          localStorage.setItem("language", "Tiếng Việt");
          localStorage.setItem("flag", "vn-flag");
          this.$i18n.locale = MResource.language.VI;
          break;
        case MResource.language.EN:
          this.currentLanguague = "English";
          this.flagClass = "uk-flag";
          this.$store.commit("changeLanguage", MResource.language.EN);
          this.$store.commit("changeCurrentLanguage", "English");
          this.$store.commit("changeCurrentFlag", "uk-flag");

          localStorage.setItem("locale", "EN");
          localStorage.setItem("language", "English");
          localStorage.setItem("flag", "uk-flag");
          this.$i18n.locale = MResource.language.EN;
          break;
        default:
          break;
      }
      this.togleLanguageContainer();
    },
    /**
     * togle laguage container
     * 	created by: Nguyễn Thiện Thắng
     *  created at: 2024/02/26
     */
    togleLanguageContainer() {
      this.isShowLanguaContainer = !this.isShowLanguaContainer;
    },
    /**
     * update value for Employee from input component if value valid
     * @param {*} type Employee Propery
     * @param {*} newValue new value updated form input component
     * 	created by: Nguyễn Thiện Thắng
     *  created at: 2024/2/20
     */
    updateValue(type, newValue) {
      if (type in this.User) {
        this.User[type] = newValue;
      }
    },

    /**
     * Check is all user props valid before submit
     * 	created by: Nguyễn Thiện Thắng
     *  created at: 2024/2/20
     */
    isAllDataValid() {
      let passwordMsg = this.$refs.passwordRef.validateInput(
        this.User.Password
      );
      let usernameMsg = this.$refs.usernameRef.validateInput(
        this.User.Username
      );
      // all required data is valid
      if (usernameMsg === "" && passwordMsg === "") {
        return true;
      }
      return false;
    },
  },
};
</script>

<style scoped>
.login-container {
  width: 100vw;
  height: 100vh;
  position: relative;
  background: url("../assets/img/loginicon/bg1.jpg");
}

/* language */

.languague-container {
  position: fixed;
  top: 24px;
  right: 24px;
  width: 120px;
  z-index: 2;
}

.current-language-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 13px;
  border-radius: 3px;
  line-height: 32px;
  cursor: pointer;
  color: #f8f8f8;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.3);
  padding: 0 16px 0 38px;
}

.curent-language {
  display: flex;
}

.vn-flag {
  background: url("../assets/img/loginicon/vietnam-flag-icon.svg");
  position: absolute;
  left: 8px;
  top: 10px;
  height: 14px;
  width: 20px;
}

.uk-flag {
  background: url("../assets/img/loginicon/uk-flag-icon.svg");
  position: absolute;
  left: 8px;
  top: 10px;
  height: 14px;
  width: 20px;
}

.option-laguage-container {
  position: relative;
  top: 5px;
  width: 120px;
  border-radius: 3px;
  /* padding: 15px 0; */
  background-color: #ffff;
  color: #212121;
  border: 1px solid rgba(255, 255, 255, 0.3);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.option-laguage {
  display: flex;
  justify-content: space-around;
  font-size: 13px;
  line-height: 32px;
  width: 100%;
}

.laguague-vi-country-option {
  margin-left: 20px;
}

.laguague-eng-country-option {
  margin-left: 7px;
}

.option-laguage :hover {
  cursor: pointer;
  /* background-color: #f2f2f2; */
}

.language-vi-flag-option {
  background: url("../assets/img/loginicon/vietnam-flag-icon.svg");
  position: absolute;
  left: 8px;
  top: 7px;
  height: 14px;
  width: 20px;
}

.language-en-flag-option {
  background: url("../assets/img/loginicon/uk-flag-icon.svg");
  position: absolute;
  left: 8px;
  top: 41px;
  height: 14px;
  width: 20px;
}
/* end language */

.form-logo {
  display: flex;
  justify-content: center;
  align-items: center;
}

.logo {
  background-image: url(https://amismisa.misacdn.net/apps/login/img/icon-amis-platform2.svg?v=20200512);
  width: 196px;
  height: 36px;
  margin-bottom: 40px;
  /* scale: 2.5; */
}

.login-form {
  position: absolute;
  top: 18%;
  left: 50%;
  transform: translateX(-50%);
  width: 400px;
  padding: 40px 48px;
  border-radius: 8px;
  background-color: #ffff;
  box-shadow: 0 12px 20px rgba(0, 0, 0, 0.12);
}
/*  login data */

.form-username,
.form-password {
  width: 100%;
  margin-bottom: 16px;
}

.errorMsg {
  margin-bottom: 16px;
  font-size: 14px;
  color: red;
}

.form-forgot-password a {
  text-decoration: none;
  color: #0103ee;
}

.form-forgot-password {
  font-size: 14px;
  line-height: 17px;
  display: block;
  text-align: left;
  text-decoration: none;
  margin-bottom: 24px;
}
/* end login data */

/*  SSO  */

.sso-title {
  display: inline-flex;
  align-items: center;
  justify-items: center;
  height: 28px;
  width: 100%;
  margin: 16px 0px;
}

.sso-title-line {
  height: 1px;
  flex: 1;
  background-color: #9ea1a5;
}

.sso-title-text {
  padding: 5px 10px;
  background-color: #fff;
  color: #9ea1a5;
  font-size: 14px;
}

.sso-method-list {
  display: flex;
  justify-content: center;
}

.sso-method-item {
  margin: 0 4px;
  cursor: pointer;
  background-position: center;
  background-size: 40px;
  background-repeat: no-repeat;
}

.form-google-login {
  background: url("../assets/img/loginicon/svg-0.svg");
  width: 40px;
  height: 40px;
}

.form-apple-login {
  background: url("../assets/img/loginicon/svg-1.svg");
  width: 40px;
  height: 40px;
}
.form-microsoft-login {
  background: url("../assets/img/loginicon/svg-2.svg");
  width: 40px;
  height: 40px;
}

/* end SSO */

.form-copy-right {
  padding-top: 14px;
  font-size: 12px;
  position: absolute;
  width: 100%;
  text-align: center;
  left: 0;
  bottom: -28px;
  color: rgba(255, 255, 255, 0.6);
}
</style>
