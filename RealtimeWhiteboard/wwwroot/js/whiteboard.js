function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

class Whiteboard {
    constructor(canvasId) {
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext('2d');
        
        // State
        this.elements = [];
        this.currentTool = 'select'; // select, pen, line, rect, text, eraser
        this.currentColor = '#1e1e1e';
        this.currentSize = 3;
        
        // Interaction State
        this.isDrawing = false;
        this.isDragging = false;
        this.selectedElementId = null;
        this.activeElement = null; // Currently being drawn or dragged
        
        // Coordinates
        this.startX = 0;
        this.startY = 0;
        this.lastX = 0;
        this.lastY = 0;
        
        // UI
        this.textInputContainer = document.getElementById('text-input-container');
        this.textInput = document.getElementById('canvas-text-input');
        
        this.init();
    }

    init() {
        this.resize();
        window.addEventListener('resize', () => this.resize());
        
        this.canvas.addEventListener('mousedown', (e) => this.handlePointerDown(e));
        this.canvas.addEventListener('mousemove', (e) => this.handlePointerMove(e));
        this.canvas.addEventListener('mouseup', (e) => this.handlePointerUp(e));
        
        this.canvas.addEventListener('touchstart', (e) => {
            if (e.target === this.canvas) e.preventDefault();
            this.handlePointerDown(e.touches[0]);
        }, { passive: false });
        this.canvas.addEventListener('touchmove', (e) => {
            if (e.target === this.canvas) e.preventDefault();
            this.handlePointerMove(e.touches[0]);
        }, { passive: false });
        this.canvas.addEventListener('touchend', (e) => this.handlePointerUp(e.changedTouches ? e.changedTouches[0] : e));

        this.textInput.addEventListener('blur', () => this.finalizeText());
        this.textInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') this.finalizeText();
        });
    }

    resize() {
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
        this.render();
    }

    getMousePos(e) {
        const rect = this.canvas.getBoundingClientRect();
        return {
            x: (e.clientX || e.pageX || 0) - rect.left,
            y: (e.clientY || e.pageY || 0) - rect.top
        };
    }

    // ==========================================
    // Interaction Handlers
    // ==========================================

    handlePointerDown(e) {
        if (!window.currentSessionId) {
            alert("Please select or create a session first!");
            return;
        }

        const pos = this.getMousePos(e);
        this.startX = pos.x;
        this.startY = pos.y;
        this.lastX = pos.x;
        this.lastY = pos.y;

        if (this.currentTool === 'select') {
            const hitElement = this.hitTest(pos.x, pos.y);
            if (hitElement) {
                this.selectedElementId = hitElement.id;
                this.isDragging = true;
                
                // Sync UI with selected element's properties
                this.currentColor = hitElement.color;
                this.currentSize = hitElement.size;
                document.getElementById('color-picker').value = hitElement.color;
                document.getElementById('brush-size').value = hitElement.size;
                document.getElementById('brush-size-val').innerText = hitElement.size;
            } else {
                this.selectedElementId = null;
                this.isDragging = false;
            }
            this.render();
            return;
        }

        if (this.currentTool === 'eraser') {
            this.eraseAt(pos.x, pos.y);
            this.isDrawing = true; // dragging eraser
            return;
        }

        if (this.currentTool === 'text') {
            this.textInputContainer.style.display = 'block';
            this.textInputContainer.style.left = `${pos.x}px`;
            this.textInputContainer.style.top = `${pos.y}px`;
            this.textInput.style.color = this.currentColor;
            this.textInput.style.fontSize = `${this.currentSize * 5 + 10}px`;
            this.textInput.value = '';
            setTimeout(() => this.textInput.focus(), 10);
            return;
        }

        // Start drawing new shape
        this.isDrawing = true;
        this.selectedElementId = null;
        this.activeElement = {
            id: generateUUID(),
            type: this.currentTool,
            color: this.currentColor,
            size: this.currentSize,
            x1: pos.x,
            y1: pos.y,
            x2: pos.x,
            y2: pos.y,
            points: [{ x: pos.x, y: pos.y }] // for pen
        };
        this.elements.push(this.activeElement);
        this.render();
    }

    handlePointerMove(e) {
        const pos = this.getMousePos(e);

        // Hover effect logic
        if (this.currentTool === 'select' || this.currentTool === 'eraser') {
            const hitElement = this.hitTest(pos.x, pos.y);
            this.canvas.style.cursor = hitElement ? (this.currentTool === 'eraser' ? 'crosshair' : 'move') : 'default';
        } else {
            this.canvas.style.cursor = 'crosshair';
        }

        if (this.isDragging && this.selectedElementId) {
            const dx = pos.x - this.lastX;
            const dy = pos.y - this.lastY;
            this.moveElement(this.selectedElementId, dx, dy);
            this.lastX = pos.x;
            this.lastY = pos.y;
            this.render();
            return;
        }

        if (this.isDrawing && this.currentTool === 'eraser') {
            this.eraseAt(pos.x, pos.y);
            return;
        }

        if (this.isDrawing && this.activeElement) {
            this.activeElement.x2 = pos.x;
            this.activeElement.y2 = pos.y;
            if (this.currentTool === 'pen') {
                this.activeElement.points.push({ x: pos.x, y: pos.y });
            }
            this.render();
        }
    }

    handlePointerUp(e) {
        if (this.isDragging) {
            this.isDragging = false;
            if (this.selectedElementId) {
                const el = this.elements.find(e => e.id === this.selectedElementId);
                this.broadcastEvent('update', el);
            }
        }

        if (this.isDrawing) {
            this.isDrawing = false;
            if (this.activeElement && this.currentTool !== 'eraser') {
                this.broadcastEvent('create', this.activeElement);
                this.activeElement = null;
            }
        }
    }

    // ==========================================
    // Tool Specific Logic
    // ==========================================

    finalizeText() {
        if (this.textInputContainer.style.display === 'none') return;
        
        const text = this.textInput.value.trim();
        this.textInputContainer.style.display = 'none';
        
        if (text) {
            const newElement = {
                id: generateUUID(),
                type: 'text',
                color: this.currentColor,
                size: this.currentSize,
                x1: this.startX,
                y1: this.startY,
                x2: 0, y2: 0,
                text: text
            };
            this.elements.push(newElement);
            this.render();
            this.broadcastEvent('create', newElement);
        }
    }

    eraseAt(x, y) {
        const hit = this.hitTest(x, y);
        if (hit) {
            this.elements = this.elements.filter(e => e.id !== hit.id);
            if (this.selectedElementId === hit.id) this.selectedElementId = null;
            this.render();
            this.broadcastEvent('delete', { id: hit.id });
        }
    }

    moveElement(id, dx, dy) {
        const el = this.elements.find(e => e.id === id);
        if (!el) return;
        el.x1 += dx; el.y1 += dy;
        el.x2 += dx; el.y2 += dy;
        if (el.points) {
            el.points.forEach(p => { p.x += dx; p.y += dy; });
        }
    }

    updateSelectedProperty(prop, value) {
        this[prop] = value; // update local state
        if (this.selectedElementId) {
            const el = this.elements.find(e => e.id === this.selectedElementId);
            if (el) {
                el[prop] = value;
                this.render();
                this.broadcastEvent('update', el);
            }
        }
    }

    // ==========================================
    // Hit Testing
    // ==========================================
    
    hitTest(x, y) {
        // Reverse iterate to click top-most element
        for (let i = this.elements.length - 1; i >= 0; i--) {
            const el = this.elements[i];
            const bounds = this.getBounds(el);
            
            // Inflate bounds slightly for easier clicking
            const padding = Math.max(10, el.size);
            if (x >= bounds.minX - padding && x <= bounds.maxX + padding &&
                y >= bounds.minY - padding && y <= bounds.maxY + padding) {
                return el;
            }
        }
        return null;
    }

    getBounds(el) {
        let minX, minY, maxX, maxY;
        if (el.type === 'pen' && el.points && el.points.length > 0) {
            minX = Math.min(...el.points.map(p => p.x));
            maxX = Math.max(...el.points.map(p => p.x));
            minY = Math.min(...el.points.map(p => p.y));
            maxY = Math.max(...el.points.map(p => p.y));
        } else if (el.type === 'text') {
            // Approximation for text bounds
            const fontSize = el.size * 5 + 10;
            this.ctx.font = `${fontSize}px Inter, sans-serif`;
            const metrics = this.ctx.measureText(el.text);
            minX = el.x1;
            maxX = el.x1 + metrics.width;
            minY = el.y1 - fontSize / 2;
            maxY = el.y1 + fontSize / 2;
        } else {
            minX = Math.min(el.x1, el.x2);
            maxX = Math.max(el.x1, el.x2);
            minY = Math.min(el.y1, el.y2);
            maxY = Math.max(el.y1, el.y2);
        }
        return { minX, minY, maxX, maxY };
    }

    // ==========================================
    // Rendering
    // ==========================================

    render() {
        this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        this.elements.forEach(el => {
            this.drawElement(el);
        });

        // Draw selection box
        if (this.selectedElementId) {
            const el = this.elements.find(e => e.id === this.selectedElementId);
            if (el) {
                const bounds = this.getBounds(el);
                this.ctx.beginPath();
                this.ctx.strokeStyle = '#1c7ed6';
                this.ctx.lineWidth = 1;
                this.ctx.setLineDash([5, 5]);
                const padding = 6;
                this.ctx.rect(
                    bounds.minX - padding, 
                    bounds.minY - padding, 
                    bounds.maxX - bounds.minX + padding * 2, 
                    bounds.maxY - bounds.minY + padding * 2
                );
                this.ctx.stroke();
                this.ctx.setLineDash([]);
            }
        }
    }

    drawElement(el) {
        this.ctx.beginPath();
        this.ctx.strokeStyle = el.color;
        this.ctx.fillStyle = el.color;
        this.ctx.lineWidth = el.size;
        this.ctx.lineCap = 'round';
        this.ctx.lineJoin = 'round';

        if (el.type === 'pen' && el.points && el.points.length > 0) {
            this.ctx.moveTo(el.points[0].x, el.points[0].y);
            for (let i = 1; i < el.points.length; i++) {
                this.ctx.lineTo(el.points[i].x, el.points[i].y);
            }
            this.ctx.stroke();
        } else if (el.type === 'line') {
            this.ctx.moveTo(el.x1, el.y1);
            this.ctx.lineTo(el.x2, el.y2);
            this.ctx.stroke();
        } else if (el.type === 'rect') {
            this.ctx.rect(el.x1, el.y1, el.x2 - el.x1, el.y2 - el.y1);
            this.ctx.stroke();
        } else if (el.type === 'text') {
            this.ctx.font = `${el.size * 5 + 10}px Inter, sans-serif`;
            this.ctx.textBaseline = 'middle';
            this.ctx.fillText(el.text, el.x1, el.y1);
        }
    }

    clear() {
        this.elements = [];
        this.selectedElementId = null;
        this.render();
    }

    // ==========================================
    // Networking
    // ==========================================

    broadcastEvent(action, elementData) {
        if (!window.onLocalDraw) return;
        // Package the action and element data into the DataJson payload
        const payload = {
            action: action,
            element: elementData
        };
        // tool is now used as action channel (e.g. 'action')
        window.onLocalDraw('action', '', 0, payload);
    }

    // Reducer for remote events
    processRemoteEvent(action, elementData) {
        if (action === 'create') {
            // Avoid duplicates
            if (!this.elements.find(e => e.id === elementData.id)) {
                this.elements.push(elementData);
            }
        } else if (action === 'update') {
            const index = this.elements.findIndex(e => e.id === elementData.id);
            if (index !== -1) {
                this.elements[index] = elementData;
            }
        } else if (action === 'delete') {
            this.elements = this.elements.filter(e => e.id !== elementData.id);
            if (this.selectedElementId === elementData.id) this.selectedElementId = null;
        }
        this.render();
    }
}

window.whiteboard = new Whiteboard('whiteboard');

// UI Listeners
document.querySelectorAll('.btn-tool').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('.btn-tool').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        window.whiteboard.currentTool = btn.dataset.tool;
        
        if (window.whiteboard.currentTool !== 'select') {
            window.whiteboard.selectedElementId = null;
            window.whiteboard.render();
        }
        if (window.whiteboard.currentTool !== 'text') {
            window.whiteboard.textInputContainer.style.display = 'none';
        }
    });
});

document.getElementById('color-picker').addEventListener('input', (e) => {
    window.whiteboard.updateSelectedProperty('currentColor', e.target.value);
});

document.getElementById('brush-size').addEventListener('input', (e) => {
    const val = parseInt(e.target.value);
    window.whiteboard.updateSelectedProperty('currentSize', val);
    document.getElementById('brush-size-val').innerText = val;
});

document.getElementById('btn-clear').addEventListener('click', () => {
    if (confirm("Clear the entire canvas?")) {
        if (window.onLocalClear) window.onLocalClear();
    }
});
