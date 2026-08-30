export function printPage() {
    window.print();
}

export function copyText(text) {
    return navigator.clipboard.writeText(text);
}
