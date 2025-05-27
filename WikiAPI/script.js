const URL_API = "http://localhost:5073/";
// Estado da aplicação
const state = {
    user: null,
    articles: [],
    currentArticle: null
};

// Elementos da DOM
const elements = {
    // Seções
    homeSection: document.getElementById('home-section'),
    aboutSection: document.getElementById('about-section'),
    articleDetailSection: document.getElementById('article-detail-section'),
    
    // Containers
    articlesContainer: document.getElementById('articles-container'),
    
    // Botões de autenticação
    authButtons: document.getElementById('auth-buttons'),
    userInfo: document.getElementById('user-info'),
    loginBtn: document.getElementById('login-btn'),
    registerBtn: document.getElementById('register-btn'),
    logoutBtn: document.getElementById('logout-btn'),
    createArticleBtn: document.getElementById('create-article-btn'),
    
    // Informações do usuário
    userEmail: document.getElementById('user-email'),
    
    // Links de navegação
    homeLink: document.getElementById('home-link'),
    aboutLink: document.getElementById('about-link'),
    
    // Detalhes do artigo
    articleDetailTitle: document.getElementById('article-detail-title'),
    articleDetailAuthor: document.getElementById('article-detail-author'),
    articleDetailDate: document.getElementById('article-detail-date'),
    articleDetailContent: document.getElementById('article-detail-content'),
    articleActions: document.getElementById('article-actions'),
    editArticleBtn: document.getElementById('edit-article-btn'),
    deleteArticleBtn: document.getElementById('delete-article-btn'),
    
    // Modais
    loginModal: document.getElementById('login-modal'),
    registerModal: document.getElementById('register-modal'),
    articleModal: document.getElementById('article-modal'),
    
    // Formulários
    loginForm: document.getElementById('login-form'),
    registerForm: document.getElementById('register-form'),
    articleForm: document.getElementById('article-form'),
    
    // Campos de formulário
    loginEmail: document.getElementById('login-email'),
    loginPassword: document.getElementById('login-password'),
    registerName: document.getElementById('register-name'),
    registerEmail: document.getElementById('register-email'),
    registerPassword: document.getElementById('register-password'),
    articleId: document.getElementById('article-id'),
    articleTitle: document.getElementById('article-title'),
    articleContent: document.getElementById('article-content'),
    
    // Botões de cancelamento
    cancelLogin: document.getElementById('cancel-login'),
    cancelRegister: document.getElementById('cancel-register'),
    cancelArticle: document.getElementById('cancel-article'),
    
    // Título do modal de artigo
    articleModalTitle: document.getElementById('article-modal-title'),
    
    // Botão de salvar artigo
    saveArticle: document.getElementById('save-article')
};

// Funções utilitárias
const utils = {
    formatDate: (dateString) => {
        const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit' };
        return new Date(dateString).toLocaleDateString('pt-BR', options);
    },
    
    showSection: (section) => {
        elements.homeSection.style.display = 'none';
        elements.aboutSection.style.display = 'none';
        elements.articleDetailSection.style.display = 'none';
        
        if (section) {
            section.style.display = 'block';
        }
    },
    
    showModal: (modal) => {
        document.querySelectorAll('.modal').forEach(m => m.style.display = 'none');
        if (modal) {
            modal.style.display = 'flex';
        }
    },
    
    clearForm: (form) => {
        if (form) {
            form.reset();
        }
    }
};

// API Service
const api = {
    baseUrl: '/api',
    
    async request(endpoint, method = 'GET', data = null) {
        const options = {
            method,
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include' // Para cookies de autenticação
        };
        
        if (data) {
            options.body = JSON.stringify(data);
        }
        
        const response = await fetch(`${this.baseUrl}${endpoint}`, options);
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Erro na requisição');
        }
        
        return response.json();
    },
    
    // Autenticação
    async login(email, password) {
        return this.request('/auth/login', 'POST', { email, password });
    },
    
    async register(name, email, password) {
        return this.request('/auth/registrar', 'POST', { nome: name, email, senha: password });
    },
    
    async logout() {
        return this.request('/auth/logout', 'POST');
    },
    
    async getUserInfo() {
        try {
            return await this.request('/auth/user');
        } catch {
            return null;
        }
    },
    
    // Artigos
    async getArticles() {
        return this.request('/artigos');
    },
    
    async getArticle(id) {
        return this.request(`/artigos/${id}`);
    },
    
    async createArticle(article) {
        return this.request('/artigos', 'POST', article);
    },
    
    async updateArticle(id, article) {
        return this.request(`/artigos/${id}`, 'PUT', article);
    },
    
    async deleteArticle(id) {
        return this.request(`/artigos/${id}`, 'DELETE');
    }
};

// Renderização
const render = {
    async articles() {
        try {
            state.articles = await api.getArticles();
            elements.articlesContainer.innerHTML = '';
            
            state.articles.forEach(article => {
                const articleElement = document.createElement('div');
                articleElement.className = 'article-card';
                articleElement.innerHTML = `
                    <div class="article-content">
                        <h3>${article.titulo}</h3>
                        <p>${article.conteudo.substring(0, 200)}${article.conteudo.length > 200 ? '...' : ''}</p>
                        <div class="article-meta">
                            ${article.autor} | ${utils.formatDate(article.dataCriacao)}
                        </div>
                        <button class="btn btn-primary" data-id="${article.id}">Ler mais</button>
                    </div>
                `;
                
                elements.articlesContainer.appendChild(articleElement);
            });
            
            // Adiciona event listeners aos botões "Ler mais"
            document.querySelectorAll('.article-card button').forEach(button => {
                button.addEventListener('click', () => {
                    const articleId = parseInt(button.getAttribute('data-id'));
                    controllers.showArticleDetail(articleId);
                });
            });
            
        } catch (error) {
            alert('Erro ao carregar artigos: ' + error.message);
        }
    },
    
    async articleDetail(articleId) {
        try {
            state.currentArticle = await api.getArticle(articleId);
            
            elements.articleDetailTitle.textContent = state.currentArticle.titulo;
            elements.articleDetailContent.textContent = state.currentArticle.conteudo;
            elements.articleDetailAuthor.textContent = `Autor: ${state.currentArticle.autor}`;
            elements.articleDetailDate.textContent = `Criado em: ${utils.formatDate(state.currentArticle.dataCriacao)}`;
            
            if (state.currentArticle.dataAtualizacao) {
                elements.articleDetailDate.textContent += ` | Atualizado em: ${utils.formatDate(state.currentArticle.dataAtualizacao)}`;
            }
            
            // Mostra ações apenas para editores
            elements.articleActions.style.display = state.user && state.user.perfil === 'Editor' ? 'flex' : 'none';
            
            utils.showSection(elements.articleDetailSection);
        } catch (error) {
            alert('Erro ao carregar artigo: ' + error.message);
        }
    },
    
    updateAuthUI() {
        if (state.user) {
            elements.authButtons.style.display = 'none';
            elements.userInfo.style.display = 'flex';
            elements.userEmail.textContent = state.user.email;
            elements.createArticleBtn.style.display = state.user.perfil === 'Editor' ? 'block' : 'none';
        } else {
            elements.authButtons.style.display = 'flex';
            elements.userInfo.style.display = 'none';
        }
    }
};

// Controladores
const controllers = {
    async init() {
        // Verifica se o usuário está logado
        try {
            state.user = await api.getUserInfo();
        } catch (error) {
            state.user = null;
        }
        
        render.updateAuthUI();
        await render.articles();
        
        // Event listeners
        this.setupEventListeners();
    },
    
    setupEventListeners() {
        // Navegação
        elements.homeLink.addEventListener('click', (e) => {
            e.preventDefault();
            utils.showSection(elements.homeSection);
            render.articles();
        });
        
        elements.aboutLink.addEventListener('click', (e) => {
            e.preventDefault();
            utils.showSection(elements.aboutSection);
        });
        
        // Autenticação
        elements.loginBtn.addEventListener('click', () => {
            utils.showModal(elements.loginModal);
        });
        
        elements.registerBtn.addEventListener('click', () => {
            utils.showModal(elements.registerModal);
        });
        
        elements.logoutBtn.addEventListener('click', async () => {
            try {
                await api.logout();
                state.user = null;
                render.updateAuthUI();
                utils.showSection(elements.homeSection);
                await render.articles();
            } catch (error) {
                alert('Erro ao fazer logout: ' + error.message);
            }
        });
        
        elements.createArticleBtn.addEventListener('click', () => {
            elements.articleModalTitle.textContent = 'Criar Novo Artigo';
            utils.clearForm(elements.articleForm);
            elements.articleId.value = '';
            utils.showModal(elements.articleModal);
        });
        
        // Formulários
        elements.loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            try {
                const user = await api.login(
                    elements.loginEmail.value,
                    elements.loginPassword.value
                );
                
                state.user = user;
                render.updateAuthUI();
                utils.showModal(null);
                utils.clearForm(elements.loginForm);
                await render.articles();
            } catch (error) {
                alert('Login falhou: ' + error.message);
            }
        });
        
        elements.registerForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            try {
                await api.register(
                    elements.registerName.value,
                    elements.registerEmail.value,
                    elements.registerPassword.value
                );
                
                alert('Registro realizado com sucesso! Faça login.');
                utils.showModal(null);
                utils.clearForm(elements.registerForm);
            } catch (error) {
                alert('Registro falhou: ' + error.message);
            }
        });
        
        elements.articleForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            try {
                const articleData = {
                    titulo: elements.articleTitle.value,
                    conteudo: elements.articleContent.value
                };
                
                if (elements.articleId.value) {
                    // Atualizar artigo existente
                    await api.updateArticle(elements.articleId.value, articleData);
                } else {
                    // Criar novo artigo
                    await api.createArticle(articleData);
                }
                
                utils.showModal(null);
                utils.clearForm(elements.articleForm);
                
                if (elements.articleDetailSection.style.display === 'block') {
                    await render.articleDetail(state.currentArticle.id);
                } else {
                    await render.articles();
                }
            } catch (error) {
                alert('Erro ao salvar artigo: ' + error.message);
            }
        });
        
        // Botões de cancelamento
        elements.cancelLogin.addEventListener('click', () => {
            utils.showModal(null);
            utils.clearForm(elements.loginForm);
        });
        
        elements.cancelRegister.addEventListener('click', () => {
            utils.showModal(null);
            utils.clearForm(elements.registerForm);
        });
        
        elements.cancelArticle.addEventListener('click', () => {
            utils.showModal(null);
            utils.clearForm(elements.articleForm);
        });
        
        // Ações de artigo
        elements.editArticleBtn.addEventListener('click', () => {
            elements.articleModalTitle.textContent = 'Editar Artigo';
            elements.articleId.value = state.currentArticle.id;
            elements.articleTitle.value = state.currentArticle.titulo;
            elements.articleContent.value = state.currentArticle.conteudo;
            utils.showModal(elements.articleModal);
        });
        
        elements.deleteArticleBtn.addEventListener('click', async () => {
            if (confirm('Tem certeza que deseja excluir este artigo?')) {
                try {
                    await api.deleteArticle(state.currentArticle.id);
                    utils.showSection(elements.homeSection);
                    await render.articles();
                } catch (error) {
                    alert('Erro ao excluir artigo: ' + error.message);
                }
            }
        });
    },
    
    async showArticleDetail(articleId) {
        await render.articleDetail(articleId);
    }
};

// Inicializa a aplicação
document.addEventListener('DOMContentLoaded', () => {
    controllers.init();
});