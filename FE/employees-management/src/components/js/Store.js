import { createStore } from "vuex"
export const store = createStore({
    state() {
        return {
            isAuthenticate: false,
            userName: ""
        }
    },
    mutations: {
        changeAuthenticateStatus(state, status) {
            state.isAuthenticate = status
        },
        changeUserNameStatus(state, newUserName) {
            state.userName = newUserName
        }
    }
})