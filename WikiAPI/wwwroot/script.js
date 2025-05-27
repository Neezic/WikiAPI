// Estado global
const AppState = {
    currentUser: null,
    articles: []
};

// Elementos DOM
const DOM = {
    loginForm: document.getElementById('login-form'),
    logoutBtn: document.getElementById('logout-btn'),
    createArticleBtn: document.getElementById('create-article-btn'),
    userSection: document.getElementById('user-section'),
    guestSection: document.getElementById('guest-section'),
    editorActions: document.getElementById('editor-actions'),
    articlesContainer: document.getElementById('articles-container')
};

// Serviços
const ApiService = {
    async fetch(url, options = {}) {
        const response = await fetch(`/api${url}`, {
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            ...options
        });
        
        if (!response.ok) throw new Error(await response.text());
        return response.json();
    }
};

const AuthService = {
     async login(email, password) {
        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include', // Para enviar cookies
                body: JSON.stringify({
                    email: email,
                    senha: password // Note o nome do campo deve bater com o backend
                })
            });

            if (!response.ok) {
                const errorData = await response.json();
                throw new Error(errorData.message || 'Login falhou');
            }

            return await response.json();
        } catch (error) {
            console.error('Erro no login:', error);
            throw error;
        }
    },
    
    async logout() {
        await ApiService.fetch('/auth/logout', { method: 'POST' });
        AppState.currentUser = null;
    },

    async checkAuth() {
        try {
            AppState.currentUser = await ApiService.fetch('/auth/check');
        } catch {
            AppState.currentUser = null;
        }
    }
};

// Renderização
const Renderer = {
    updateUI() {
        if (!DOM.userSection || !DOM.guestSection) return;
        
        DOM.userSection.style.display = AppState.currentUser ? 'block' : 'none';
        DOM.guestSection.style.display = AppState.currentUser ? 'none' : 'block';
        
        if (DOM.editorActions && AppState.currentUser) {
            DOM.editorActions.style.display = 
                AppState.currentUser.profile === 'Editor' ? 'block' : 'none';
        }
    },

    renderArticles() {
        if (!DOM.articlesContainer) return;
        
        DOM.articlesContainer.innerHTML = AppState.articles
            .map(article => `
                <div class="article">
                    <h3>${article.titulo}</h3>
                    <p>${article.conteudo.substring(0, 100)}...</p>
                    <small>Por ${article.autor}</small>
                </div>
            `)
            .join('');
    }
};

// Inicialização
const App = {
    async init() {
        await AuthService.checkAuth();
        Renderer.updateUI();
        
        if (AppState.currentUser) {
            await this.loadArticles();
        }
        
        this.bindEvents();
    },

    async loadArticles() {
        try {
            AppState.articles = await ApiService.fetch('/articles');
            Renderer.renderArticles();
        } catch (error) {
            console.error('Falha ao carregar artigos:', error);
        }
    },

    bindEvents() {
        if (DOM.loginForm) {
            DOM.loginForm.addEventListener('submit', async (e) => {
                e.preventDefault();
                const email = e.target.elements['login-email'].value;
                const password = e.target.elements['login-password'].value;
                
                try {
                    await AuthService.login(email, password);
                    await this.init(); // Recarrega o estado completo
                } catch (error) {
                    alert('Login falhou: ' + error.message);
                }
            });
        }

        if (DOM.logoutBtn) {
            DOM.logoutBtn.addEventListener('click', async () => {
                await AuthService.logout();
                await this.init();
            });
        }
    }
};

// Inicia quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', () => App.init());