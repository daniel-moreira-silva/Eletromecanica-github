import router from '@/router'

class StorageService {
    async validarToken() {
        if (!localStorage.getItem("loginNovoSanegeo")) {
            localStorage.setItem("rotaNavegacao", router.currentRoute._value.fullPath);
            router.push({ path: "/" }).catch(failure => { localStorage.setItem("erro", failure) });
            return false;
        }
        return true;
    }
}

export default StorageService;