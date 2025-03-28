console.log("scroll.js loaded successfully!");

const scrollContainer = document.querySelector('.overflow-hidden');
const scrollAmount = 400; // Increased scroll amount for better movement

function smoothScroll(direction) {
    const currentScroll = scrollContainer.scrollLeft;
    const targetScroll = currentScroll + (direction * scrollAmount);
    // Ensure we don't scroll past the beginning
    if (targetScroll < 0) {
        scrollContainer.scrollTo({
            left: 0,
            behavior: 'smooth'
        });
    } else {
        scrollContainer.scrollTo({
            left: targetScroll,
            behavior: 'smooth'
        });
    }
}

function scrollLeftward() {
    smoothScroll(-1);
}

function scrollRightward() {
    smoothScroll(1);
}