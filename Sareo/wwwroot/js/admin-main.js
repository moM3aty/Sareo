document.addEventListener('DOMContentLoaded', function () {
    // Function to convert English numbers to Arabic numerals
    const toArabicNumerals = (str) => {
        if (str === null || str === undefined) return '';
        return str.toString().replace(/\d/g, d => '٠١٢٣٤٥٦٧٨٩'[d]);
    };

    // Function to format date string to a long Arabic date
    const formatArabicDate = (dateString) => {
        const date = new Date(dateString);
        if (isNaN(date)) return toArabicNumerals(dateString); // Return original converted if invalid

        const options = {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        };
        // Using "ar-EG" for Egyptian Arabic locale which uses Arabic numerals
        return toArabicNumerals(date.toLocaleDateString("ar-EG", options));
    };

    // Function to format date and time string
    const formatArabicDateTime = (dateTimeString) => {
        const date = new Date(dateTimeString);
        if (isNaN(date)) return toArabicNumerals(dateTimeString);

        return toArabicNumerals(date.toLocaleString("ar-EG", {
            year: 'numeric',
            month: 'numeric',
            day: 'numeric',
            hour: 'numeric',
            minute: '2-digit'
        }));
    };


    // Function to apply localization to the whole page
    const localizePageContent = () => {
        const allTextNodes = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, null, false);
        let node;
        while (node = allTextNodes.nextNode()) {
            if (node.parentElement.tagName !== 'SCRIPT' && node.parentElement.tagName !== 'STYLE') {
                // Regex to find standalone numbers, percentages, and dates in YYYY/MM/DD or YYYY/MM/DD hh:mm tt format
                const regex = /\b\d{4}\/\d{2}\/\d{2}(?:\s\d{1,2}:\d{2}\s(?:AM|PM))?\b|\b\d+\b/g;
                if (regex.test(node.nodeValue)) {
                    node.nodeValue = node.nodeValue.replace(regex, (match) => {
                        if (match.includes('/')) {
                            if (match.includes(':')) {
                                return formatArabicDateTime(match);
                            }
                            return formatArabicDate(match);
                        }
                        return toArabicNumerals(match);
                    });
                }
            }
        }
    };

    // Apply localization on initial load
    localizePageContent();
});
