import { createApp } from 'vue'
import { createStore } from 'vuex'
import { createI18n } from 'vue-i18n';
// mulitple language
import EN from './locale/en.json'
import VI from './locale/vi.json'
import App from './App.vue'
// lib 
import axios from 'axios';
import tinyEmitter from 'tiny-emitter/instance'
import { saveAs } from 'file-saver';
import router from './components/js/Router.js';
// vuex
const store = createStore({
    state() {
        return {
            isAuthenticate: !(!localStorage.getItem("expirationToken")),
            userName: localStorage.getItem("username") ? localStorage.getItem("username") : "",
            locale: localStorage.getItem("locale") ? localStorage.getItem("locale") : "VI",
            currentLanguague: localStorage.getItem("language") ? localStorage.getItem("language") : "Tiếng Việt",
            currentFlag: localStorage.getItem("flag") ? localStorage.getItem("flag") : "vn-flag",
        }
    },
    mutations: {
        changeAuthenticateStatus(state, status) {
            state.isAuthenticate = status
        },
        changeUserNameStatus(state, newUserName) {
            state.userName = newUserName
        },
        changeLanguage(state, language) {
            state.locale = language
        },
        changeCurrentLanguage(state, newLanguage) {
            state.currentLanguague = newLanguage
        },
        changeCurrentFlag(state, newCurrentFlag) {
            state.currentFlag = newCurrentFlag
        },
    }
});


const i18n = createI18n({
    locale: store.state.locale,
    messages: {
        EN: EN,
        VI: VI
    }
})

//resource
import MResource from './components/js/StringResource';
import Enum from './components/js/Enum';

const app = createApp(App);
app.config.globalProperties.emitter = tinyEmitter;
app.config.globalProperties.api = axios;
app.config.globalProperties.resource = MResource;
app.config.globalProperties.enum = Enum;
app.config.globalProperties.saveAs = saveAs;
app.use(router);
app.use(store);
app.use(i18n)

app.mount('#app')
export { store };
export { i18n };
export default { app }; 