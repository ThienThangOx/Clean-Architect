import { createRouter, createWebHistory } from 'vue-router'
import { store } from "@/main";
import TheLogin from '../layout/TheLogin.vue'
import TheLayout from '../layout/TheLayout'
import EmployeeList from '../layout/main/views/EmployeeList'
import EmployeeCash from '../layout/main/views/EmployeeCash'
import EmployeeMoney from '../layout/main/views/EmployeeMoney'
import EmployeeeOther from '../layout/main/views/EmployeeOther'
import TheNotFound from '../layout/TheNotFound'

const routes = [
    { path: '/', redirect: '/login' },
    { path: '/login', component: TheLogin },
    { path: '/employee-layout', redirect: '/employee-list' },
    { path: '/not-found', component: TheNotFound },
    {
        path: '/employee-layout',
        component: TheLayout,
        children: [
            { path: '/employee-list', component: EmployeeList },
            { path: '/employee-cash', component: EmployeeCash },
            { path: '/employee-money', component: EmployeeMoney },
            { path: '/employee-other', component: EmployeeeOther }
        ]
    },
    {
        path: "/:pathMatch(.*)*",
        component: TheNotFound,
    }
];


const router = createRouter({

    history: createWebHistory(),
    routes,
    linkActiveClass: "sidebar-option-element-target"
});
router.beforeEach((to, from, next) => {
    console.log(to.path);
    if (!store.state.isAuthenticate && to.path !== '/login') {
        next("/login");
    } else {
        next();
    }
});

export default router;
