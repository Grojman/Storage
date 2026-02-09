const JSON_PATH = "repository-data.json";

let container;
let languageTemplate;
let lastClicked;
let lastCodeClicked;

document.addEventListener('DOMContentLoaded', () => {
    container = document.querySelector('.languages-container')
    languageTemplate = document.querySelector('#language-template')
    
    loadLanguages();
});

async function loadLanguages() {
    const res = await fetch(JSON_PATH);
    const data = await res.json();
    
    data.forEach(lang => {
        const node = createLanguageNode(lang);
        container.appendChild(node);
    });
}

function createLanguageNode(languageData) {
    const clone = languageTemplate.content.cloneNode(true);
    const card = clone.querySelector('.card');
    
    const nameEl = clone.querySelector('.language-name');
    const imgEl = clone.querySelector('.language-image');
    const filesList = clone.querySelector('.files-list');
    const backBtn = clone.querySelector('.back-btn');
    const contentEl = clone.querySelector('.file-content code');
    const fileNameEl = clone.querySelector('.file-name');
    const copyBtn = clone.querySelector('.copy-btn');
    
    nameEl.textContent = languageData.name;
    imgEl.src = languageData.icon;
    imgEl.alt = languageData.name;
    
    
    // Fill file list
    renderItems(languageData.items, filesList, contentEl, fileNameEl, copyBtn);
    
    const newLangClass = "language-" + languageData.name.toLowerCase();

    contentEl.classList.add(newLangClass);

    // Expand on click
    card.addEventListener('click', () => {
        if (card.classList.contains('expanded')) return;
        card.classList.add('expanded');
        document.body.style.overflow = 'hidden';
    });
    
    // Back button
    backBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        card.classList.remove('expanded');
        document.body.style.overflow = '';
    });
    
    return clone;
}

function renderItems(items, container, contentEl, fileNameEl, copyBtn) {
    items.forEach(item => {
        if (item.type === "folder") {
            const folder = document.createElement('div');
            folder.classList.add('folder');

            const header = document.createElement('div');
            header.classList.add('folder-header');
            header.textContent = "📁 " + item.name;

            const children = document.createElement('div');
            children.classList.add('folder-children');

            header.addEventListener('click', (e) => {
                e.stopPropagation();
                children.classList.toggle('open');
            });

            folder.appendChild(header);
            folder.appendChild(children);
            container.appendChild(folder);

            renderItems(item.items, children, contentEl, fileNameEl, copyBtn);
        }

        if (item.type === "file") {
            const fileNode = document.createElement('div');
            fileNode.classList.add('file');

            const title = document.createElement('div');
            title.classList.add('file-title');
            title.textContent = item.name;

            const desc = document.createElement('p');
            desc.classList.add('file-description');
            desc.textContent = item.description || "No description available";

            fileNode.appendChild(title);
            fileNode.appendChild(desc);

            fileNode.addEventListener('click', async (e) => {
                if (lastClicked)
                {
                    lastClicked.classList.remove('expanded');
                }
                lastClicked = fileNode;
                fileNode.classList.add('expanded')

                e.stopPropagation();

                contentEl.textContent = "Loading file...";
                fileNameEl.textContent = item.path.split('/').pop();

                try {
                    const res = await fetch(item.path);
                    const text = await res.text();

                    contentEl.textContent = text;
                    if (lastCodeClicked)
                    {
                        delete lastCodeClicked.dataset.highlighted
                    }
                    
                    lastCodeClicked = contentEl;
                    hljs.highlightElement(contentEl);

                    copyBtn.dataset.code = text;
                } catch {
                    contentEl.textContent = "Error loading file.";
                }
            });

            container.appendChild(fileNode);
        }
    });
}
