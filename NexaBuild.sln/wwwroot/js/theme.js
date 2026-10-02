// ==========================================================================
// NEXABUILD CORE JAVASCRIPT & INTERACTIONS
// ==========================================================================

document.addEventListener('DOMContentLoaded', () => {
    // 1. Navbar Scroll Effect
    const navbar = document.querySelector('.nb-navbar');
    if (navbar) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 40) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        });
    }

    // 2. Project Category Filtering
    const filterButtons = document.querySelectorAll('.filter-btn');
    const projectCards = document.querySelectorAll('.project-item');

    filterButtons.forEach(btn => {
        btn.addEventListener('click', () => {
            filterButtons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            const filterValue = btn.getAttribute('data-filter');

            projectCards.forEach(card => {
                if (filterValue === 'all' || card.getAttribute('data-category') === filterValue) {
                    card.style.display = 'block';
                    setTimeout(() => {
                        card.style.opacity = '1';
                        card.style.transform = 'translateY(0)';
                    }, 50);
                } else {
                    card.style.opacity = '0';
                    card.style.transform = 'translateY(20px)';
                    setTimeout(() => {
                        card.style.display = 'none';
                    }, 300);
                }
            });
        });
    });

    // 3. Favorite Button AJAX Toggle
    const favButtons = document.querySelectorAll('.project-fav-btn');
    favButtons.forEach(btn => {
        btn.addEventListener('click', async (e) => {
            e.preventDefault();
            e.stopPropagation();

            const projectId = btn.getAttribute('data-project-id');
            if (!projectId) return;

            try {
                const response = await fetch(`/Account/ToggleFavorite?projectId=${projectId}`, {
                    method: 'POST',
                    headers: {
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || '',
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                if (response.status === 401) {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    return;
                }

                const result = await response.json();
                if (result.success) {
                    if (result.isFavorited) {
                        btn.classList.add('active');
                        btn.innerHTML = '<i class="bi bi-heart-fill"></i>';
                        showToast('Başarılı', 'Proje favorilerinize eklendi.', 'success');
                    } else {
                        btn.classList.remove('active');
                        btn.innerHTML = '<i class="bi bi-heart"></i>';
                        showToast('Bilgi', 'Proje favorilerinizden kaldırıldı.', 'info');
                    }
                }
            } catch (err) {
                console.error('Favorite toggle error:', err);
            }
        });
    });

    // 4. Progress Bars Animation on Scroll
    const progressBars = document.querySelectorAll('.nb-progress-bar');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const bar = entry.target;
                const width = bar.getAttribute('data-progress') || '0';
                bar.style.width = width + '%';
            }
        });
    }, { threshold: 0.2 });

    progressBars.forEach(bar => observer.observe(bar));
});

// Toast Notification Helper
function showToast(title, message, type = 'success') {
    let container = document.getElementById('nb-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'nb-toast-container';
        container.className = 'nb-alert-toast';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    const bgClass = type === 'success' ? 'bg-dark text-white border-gold' : 'bg-dark text-white';
    toast.className = `toast show ${bgClass} p-3 mb-2 shadow-lg border`;
    toast.style.borderColor = 'var(--nb-gold-500)';
    toast.innerHTML = `
        <div class="d-flex align-items-center justify-content-between mb-1">
            <strong style="color: var(--nb-gold-500);"><i class="bi bi-info-circle-fill me-2"></i>${title}</strong>
            <button type="button" class="btn-close btn-close-white" onclick="this.parentElement.parentElement.remove()"></button>
        </div>
        <div class="text-light small">${message}</div>
    `;

    container.appendChild(toast);
    setTimeout(() => {
        toast.remove();
    }, 4500);
}
