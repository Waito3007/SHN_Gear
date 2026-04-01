interface FlyToCartOptions {
  fromElement: HTMLElement | null;
  imageUrl?: string;
  fallbackText?: string;
}

function getVisibleCartIcon() {
  const nodes = Array.from(document.querySelectorAll<HTMLElement>('[data-cart-icon]'));
  return nodes.find((node) => {
    const rect = node.getBoundingClientRect();
    return rect.width > 0 && rect.height > 0;
  });
}

export function bumpCartIcon() {
  const cartIcon = getVisibleCartIcon();
  if (!cartIcon) return;

  cartIcon.classList.add('cart-bump');
  setTimeout(() => cartIcon.classList.remove('cart-bump'), 450);
}

export function animateFlyToCart({ fromElement, imageUrl, fallbackText = 'P' }: FlyToCartOptions) {
  const cartIcon = getVisibleCartIcon();
  if (!cartIcon) return;

  if (!fromElement) {
    bumpCartIcon();
    return;
  }

  const fromRect = fromElement.getBoundingClientRect();
  const toRect = cartIcon.getBoundingClientRect();

  const ghost = document.createElement('div');
  ghost.style.position = 'fixed';
  ghost.style.left = `${fromRect.left + fromRect.width / 2 - 26}px`;
  ghost.style.top = `${fromRect.top + fromRect.height / 2 - 26}px`;
  ghost.style.width = '52px';
  ghost.style.height = '52px';
  ghost.style.borderRadius = '999px';
  ghost.style.zIndex = '9999';
  ghost.style.pointerEvents = 'none';
  ghost.style.overflow = 'hidden';
  ghost.style.border = '2px solid rgba(255,255,255,0.9)';
  ghost.style.boxShadow = '0 12px 24px rgba(0,0,0,0.18)';
  ghost.style.background = 'linear-gradient(145deg, #f9fafb 0%, #e5e7eb 100%)';

  if (imageUrl) {
    const img = document.createElement('img');
    img.src = imageUrl;
    img.alt = 'product';
    img.style.width = '100%';
    img.style.height = '100%';
    img.style.objectFit = 'cover';
    ghost.appendChild(img);
  } else {
    const text = document.createElement('span');
    text.textContent = fallbackText.toUpperCase();
    text.style.display = 'flex';
    text.style.alignItems = 'center';
    text.style.justifyContent = 'center';
    text.style.width = '100%';
    text.style.height = '100%';
    text.style.fontWeight = '700';
    text.style.color = '#6b7280';
    ghost.appendChild(text);
  }

  document.body.appendChild(ghost);

  const translateX = toRect.left + toRect.width / 2 - (fromRect.left + fromRect.width / 2);
  const translateY = toRect.top + toRect.height / 2 - (fromRect.top + fromRect.height / 2);

  requestAnimationFrame(() => {
    ghost.style.transition = 'transform 700ms cubic-bezier(0.2, 0.8, 0.2, 1), opacity 700ms ease-in-out';
    ghost.style.transform = `translate(${translateX}px, ${translateY}px) scale(0.2)`;
    ghost.style.opacity = '0.25';
  });

  bumpCartIcon();

  const cleanup = () => {
    ghost.removeEventListener('transitionend', cleanup);
    if (document.body.contains(ghost)) {
      document.body.removeChild(ghost);
    }
  };

  ghost.addEventListener('transitionend', cleanup);
}
