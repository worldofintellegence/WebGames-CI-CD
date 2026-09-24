// CIEL Web Games Catalog - Portal JavaScript Engine
document.addEventListener('DOMContentLoaded', () => {
    let allGames = [];
    let currentCategory = 'all';

    const gamesGrid = document.getElementById('gamesGrid');
    const searchInput = document.getElementById('searchInput');
    const categoryFilters = document.getElementById('categoryFilters');
    const gameCountEl = document.getElementById('gameCount');
    const emptyState = document.getElementById('emptyState');

    // Fetch games catalog from games.json
    async function loadGames() {
        try {
            const response = await fetch('games.json?t=' + Date.now());
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            allGames = await response.json();
            renderGames();
        } catch (error) {
            console.warn('[Portal] Unable to fetch games.json. Falling back to default list or sample games.', error);
            allGames = [
                {
                    gameId: "ciel-minigame-template",
                    productName: "Car Racing",
                    description: "High-performance WebGL mini-game with Google Ads integration, responsive UI, and interactive controls.",
                    category: "Arcade",
                    tags: ["webgl", "unity", "racing", "arcade"],
                    branch: "main",
                    path: "./",
                    icon: "Assets/Icons/app-icon.png",
                    adsSupported: true
                }
            ];
            renderGames();
        }
    }

    // Render games based on active search and category
    function renderGames() {
        const query = searchInput.value.toLowerCase().trim();
        
        const filteredGames = allGames.filter(game => {
            const matchesCategory = (currentCategory === 'all') || 
                                    (game.category && game.category.toLowerCase() === currentCategory.toLowerCase());

            const title = (game.productName || game.gameId || '').toLowerCase();
            const desc = (game.description || '').toLowerCase();
            const category = (game.category || '').toLowerCase();
            const tags = (game.tags || []).join(' ').toLowerCase();

            const matchesSearch = !query || 
                                  title.includes(query) || 
                                  desc.includes(query) || 
                                  category.includes(query) || 
                                  tags.includes(query);

            return matchesCategory && matchesSearch;
        });

        // Update count
        gameCountEl.textContent = filteredGames.length;

        if (filteredGames.length === 0) {
            gamesGrid.innerHTML = '';
            emptyState.classList.remove('hidden');
            return;
        }

        emptyState.classList.add('hidden');
        
        gamesGrid.innerHTML = filteredGames.map(game => {
            const iconUrl = game.icon || 'assets/default-icon.png';
            const gamePath = game.path || `${game.branch || game.gameId}/`;
            const tagsHtml = (game.tags || []).map(t => `<span class="tag-chip">#${t}</span>`).join('');
            
            return `
                <article class="game-card">
                    <img src="${iconUrl}" alt="${game.productName}" class="card-thumbnail" onerror="this.onerror=null; this.src='https://placehold.co/400x250/1e293b/6366f1?text=${encodeURIComponent(game.productName || 'WebGL Game')}';">
                    <div class="card-body">
                        <div class="card-header">
                            <h2 class="game-title">${game.productName || game.gameId}</h2>
                            <span class="category-tag">${game.category || 'Arcade'}</span>
                        </div>
                        <p class="game-desc">${game.description || 'Interactive WebGL game published via CIEL Web Games Automation.'}</p>
                        <div class="game-tags">${tagsHtml}</div>
                        <a href="${gamePath}" class="play-btn">
                            <span class="play-icon">▶</span> Play Now
                        </a>
                    </div>
                </article>
            `;
        }).join('');
    }

    // Category Filter Event Listeners
    categoryFilters.addEventListener('click', (e) => {
        if (!e.target.classList.contains('filter-btn')) return;

        document.querySelectorAll('.filter-btn').forEach(btn => btn.classList.remove('active'));
        e.target.classList.add('active');
        currentCategory = e.target.getAttribute('data-category');
        renderGames();
    });

    // Search Input Listener
    searchInput.addEventListener('input', () => {
        renderGames();
    });

    // Initial load
    loadGames();
});
