// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Função para atualizar o contador do carrinho via AJAX
function updateCartCount() {
    fetch('/Cart/GetCartCount')
        .then(response => response.json())
        .then(data => {
            const cartBadge = document.getElementById('cart-badge');
            if (cartBadge) {
                cartBadge.textContent = data;
                
                // Adiciona animação ao atualizar o contador
                cartBadge.classList.add('cart-count-update');
                setTimeout(() => {
                    cartBadge.classList.remove('cart-count-update');
                }, 500);
            }
        })
        .catch(error => console.error('Erro ao atualizar o contador do carrinho:', error));
}

// Atualizar o contador do carrinho quando a página carrega
document.addEventListener('DOMContentLoaded', function () {
    // Atualiza o contador do carrinho ao carregar a página
    updateCartCount();
});
