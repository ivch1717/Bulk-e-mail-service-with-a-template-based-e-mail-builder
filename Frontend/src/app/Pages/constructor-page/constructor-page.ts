import {ChangeDetectorRef, Component, ElementRef, ViewChild} from '@angular/core';
import {HtmlBlock} from '../../Components/html-block/html-block';
import { HttpClient } from '@angular/common/http';
import {Router} from '@angular/router';
import {CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray} from '@angular/cdk/drag-drop';
import {CdkScrollable} from '@angular/cdk/overlay';
import {TemplateTransferService} from '../../Services/template-transfer/template-transfer';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatToolbarModule} from '@angular/material/toolbar';
import {MatTooltipModule} from '@angular/material/tooltip';

type Block = {
  id: number,
  html: string,
  name: string,
  isNew?: boolean,
  dropTick?: number,
  upTick?: number
};

@Component({
  selector: 'app-constructor-page',
  imports: [
    HtmlBlock,
    CdkDropList,
    CdkDrag,
    CdkScrollable,
    MatButtonModule,
    MatIconModule,
    MatToolbarModule,
    MatTooltipModule
  ],
  templateUrl: './constructor-page.html',
  styleUrl: './constructor-page.css',
  standalone: true
})
export class ConstructorPage {
  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private router: Router,
    private templateTransferService: TemplateTransferService
  ) {}

  blocks: Block[] = [];
  templateName: string = 'template';
  private nextId = 1;
  activeBlockId: number | null = null;
  dropAnimationTick = 0;
  upAnimationTick = 0;

  back(){
    this.router.navigate(['/']);
  }
  addBlock() {
    let newBlock: Block = { id: this.nextId++, html: '', name: 'block', isNew: true };
    this.blocks.push(newBlock);

    setTimeout(() => {
      this.blocks = this.blocks.map(b => ({
        ...b,
        isNew: false
      }));
    }, 450);
  }

  addNextBlock() {
    if (this.activeBlockId === null) return;

    const index = this.blocks.findIndex(b => b.id === this.activeBlockId);
    if (index === -1) return;

    const newBlock: Block = {
      id: this.nextId++,
      html: '',
      name: 'block',
      isNew: true,
      dropTick: undefined
    };

    this.blocks = [
      ...this.blocks.slice(0, index + 1).map(b => ({
        ...b,
        dropTick: undefined
      })),
      newBlock,
      ...this.blocks.slice(index + 1).map(b => ({
        ...b,
        dropTick: undefined
      }))
    ];

    this.cdr.detectChanges();

    requestAnimationFrame(() => {
      const tick = ++this.dropAnimationTick;

      this.blocks = this.blocks.map((b, i) => ({
        ...b,
        dropTick: i > index + 1 ? tick : undefined
      }));

      this.cdr.detectChanges();
    });

    setTimeout(() => {
      this.blocks = this.blocks.map(b => ({
        ...b,
        isNew: false
      }));
    }, 450);
  }

  drop(event: CdkDragDrop<Block[]>) {
    if (event.previousIndex === event.currentIndex) return;

    const updated = [...this.blocks];
    moveItemInArray(updated, event.previousIndex, event.currentIndex);
    this.blocks = updated;
  }

  deleteBlock() {
    if (this.activeBlockId === null) return;

    const index = this.blocks.findIndex(b => b.id === this.activeBlockId);
    if (index === -1) return;

    const deletedId = this.activeBlockId;

    this.blocks = this.blocks
      .filter(b => b.id !== deletedId)
      .map(b => ({
        ...b,
        upTick: undefined
      }));

    this.activeBlockId = null;

    this.cdr.detectChanges();

    requestAnimationFrame(() => {
      const tick = ++this.upAnimationTick;

      this.blocks = this.blocks.map((b, i) => ({
        ...b,
        upTick: i >= index ? tick : undefined
      }));

      this.cdr.detectChanges();
    });

    setTimeout(() => {
      this.blocks = this.blocks.map(b => ({
        ...b,
        upTick: undefined
      }));
    }, 550);
  }

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  importBlock() {
    this.fileInput.nativeElement.click();
  }

  @ViewChild('templateFileInput') templateFileInput!: ElementRef<HTMLInputElement>;
  importTemplate() {
    this.templateFileInput.nativeElement.click();
  }

  async onTemplateFileSelected(event?: Event) {
    if (!event) return;

    const input = event.target as HTMLInputElement | null;
    if (!input || !input.files?.length) return;

    const file = input.files[0];
    const text = await file.text();
    const d = text.split('<!-- block: ');
    this.templateName = file.name.replace(/\.html?$/i, '');

    this.blocks = [];
    if (d.length > 1) {
      for (let i = 1; i < d.length; i++) {
        const separatorIndex = d[i].indexOf('-->');
        const name = d[i].substring(0, separatorIndex).trim();
        const html = d[i].substring(separatorIndex + 3).trim();
        this.blocks.push({ id: this.nextId++, html, name, isNew: true });
      }
    } else {
      this.blocks.push({ id: this.nextId++, html: text.trim(), name: "block", isNew: true });
    }
    this.cdr.detectChanges();
    input.value = '';

    setTimeout(() => {
      this.blocks = this.blocks.map(b => ({ ...b, isNew: false }));
    }, 450);
  }


  exportBlock() {
    if (this.activeBlockId === null) return;

    const block = this.blocks.find(b => b.id === this.activeBlockId);
    if (!block) return;

    if (block.html.length == 0){
      alert('Блок пустой')
      return;
    }

    this.http.post(
      '/blocks/export',
      { html: block.html },
      { responseType: 'blob' }
    ).subscribe(blob => {

      const url = window.URL.createObjectURL(blob);

      const a = document.createElement('a');
      a.href = url;
      a.download = (block.name.trim() || 'block') + '.html';
      a.click();

      window.URL.revokeObjectURL(url);
    });
  }

  onBlockNameChange(id: number, name: string) {
    this.blocks = this.blocks.map(b =>
      b.id === id ? { ...b, name } : b
    );
  }

  getTextTemplate(){
    let template: string = '';
    for (let i = 0; i < this.blocks.length; i++) {
      if (this.blocks[i].html.length > 0){
        template += `<!-- block: ${this.blocks[i].name} -->\n` + this.blocks[i].html + '\n';
      }
    }
    return template;
  }

  exportTemplate(){
    let template: string = this.getTextTemplate()
    if (template === '') {
      alert("Ошибка, шаблон пустой")
      return;
    }

    this.http.post(
      '/templates/export',
      { html: template },
      { responseType: 'blob' }
    ).subscribe(blob => {

      const url = window.URL.createObjectURL(blob);

      const a = document.createElement('a');
      a.href = url;
      a.download = (this.templateName.trim() || 'template') + '.html';
      a.click();

      window.URL.revokeObjectURL(url);
    });
  }

  viewTemplate(){
    let template: string = this.getTextTemplate()
    if (template === '') {
      alert("Ошибка, шаблон пустой")
      return;
    }

    this.http.post(
      '/templates/export',
      { html: template },
      { responseType: 'blob' }
    ).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      window.open(url, '_blank');

      setTimeout(() => {
        window.URL.revokeObjectURL(url);
      }, 1000);
    });
  }

  sendTemplate(){
    let template: string = this.getTextTemplate()
    if (template === '') {
      alert("Ошибка, шаблон пустой")
      return;
    }

    this.http.post(
      '/templates/export',
      { html: template },
      { responseType: 'blob' }
    ).subscribe(blob => {
      const file = new File([blob], this.templateName, { type: 'text/html' });

      this.templateTransferService.templateFile = file;
      this.router.navigate(['/preparation']);

    });
  }


  async onFileSelected(event?: Event) {
    if (!event) return;

    const input = event.target as HTMLInputElement | null;
    if (!input || !input.files?.length) return;

    const file = input.files[0];
    const text = await file.text();
    const name = file.name.replace(/\.html?$/i, '');

    this.blocks = [...this.blocks, { id: this.nextId++, html: text, name: name, isNew: true }];

    this.cdr.detectChanges();

    input.value = '';

    setTimeout(() => {
      this.blocks = this.blocks.map(b => ({
        ...b,
        isNew: false
      }));
    }, 450);
  }

  onBlockHtmlChange(id: string | number, html: string) {
    this.blocks = this.blocks.map(b =>
      b.id === id ? { ...b, html } : b
    );
  }

  trackById = (_: number, b: Block) => b.id;
}
