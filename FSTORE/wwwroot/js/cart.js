document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.add-to-cart').forEach(button => {
        button.addEventListener('click', () => {
            const card = button.closest('.product-card') || button;
            const item = {
                productId: card.dataset.id,
                name: card.dataset.name,
                price: parseInt(card.dataset.price),
                quantity: 1,
                imageUrl: card.dataset.image,
                selected: true
            };

            let cart = JSON.parse(localStorage.getItem('cart')) || [];
            const existing = cart.find(p => p.productId === item.productId);
            if (existing) {
                existing.quantity += 1;
            } else {
                cart.push(item);
            }

            localStorage.setItem('cart', JSON.stringify(cart));
            alert(`Đã thêm "${item.name}" vào giỏ hàng!`);
        });
    });
});
