const JSON_PATH = "repository-data.json";

let container;
let languageTemplate;
let lastClicked;

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
    languageData.files.forEach(file => {
        const fileNode = document.createElement('div');
        fileNode.classList.add('file');
        
        // Título
        const title = document.createElement('div');
        title.classList.add('file-title');
        title.textContent = file.name;
        
        // Descripción
        const desc = document.createElement('p');
        desc.classList.add('file-description');
        desc.textContent = file.description || "No description available";
        
        fileNode.appendChild(title);
        fileNode.appendChild(desc);
        
        let descriptionLoaded = false;
        
        // Click event to load the file content
        fileNode.addEventListener('click', async (e) => {
            if(lastClicked)
                {
                lastClicked.classList.toggle('expanded');
            }
            lastClicked = fileNode;
            lastClicked.classList.toggle('expanded');
            e.stopPropagation();
            
            contentEl.textContent = "Loading file...";
            const fullFileName = file.path.split('/').pop();
            fileNameEl.textContent = fullFileName;
            
            if (!descriptionLoaded) {
                try {
                    const res = await fetch(`${file.path}.txt`);
                    
                    if (res.ok) {
                        const text = await res.text();
                        desc.textContent = text;
                    } else {
                        desc.textContent = "No description available.";
                    }
                    
                } catch {
                    desc.textContent = "No description available.";
                }
                
                descriptionLoaded = true;
            }
            
            try {
                const res = await fetch(file.path);
                const text = await res.text();
                
                contentEl.textContent = text;
                if (window.hljs) {
                    contentEl.classList.remove('hljs');
                    delete contentEl.dataset.highlighted;
                    
                    hljs.highlightElement(contentEl);
                    
                } else {
                    console.warn("highlight.js not loaded");
                }
                
                copyBtn.dataset.code = text;
                
            } catch (err) {
                contentEl.textContent = "Error loading file.";
                console.error(err);
            }
        });
        
        copyBtn.addEventListener('click', async (e) => {
            e.stopPropagation();
            
            const code = copyBtn.dataset.code;
            if (!code) return;
            
            try {
                await navigator.clipboard.writeText(code);
                copyBtn.textContent = "Copied!";
                
                setTimeout(() => {
                    copyBtn.textContent = "Copy code";
                }, 1500);
                
            } catch (err) {
                console.error("Copy failed:", err);
                copyBtn.textContent = "Failed";
            }
        });
        
        
        filesList.appendChild(fileNode);
    });
    
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
