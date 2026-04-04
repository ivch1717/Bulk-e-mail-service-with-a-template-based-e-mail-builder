import {ChangeDetectorRef, Component, ElementRef, ViewChild} from '@angular/core';
import {HtmlBlock} from '../../Components/html-block/html-block';
import { HttpClient } from '@angular/common/http';
import {Router} from '@angular/router';
import {CdkDrag, CdkDragDrop, CdkDropList, moveItemInArray} from '@angular/cdk/drag-drop';
import {CdkScrollable} from '@angular/cdk/overlay';

type Block = {
  id: number,
  html: string,
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
    CdkScrollable
  ],
  templateUrl: './constructor-page.html',
  styleUrl: './constructor-page.css',
})
export class ConstructorPage {
  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  blocks: Block[] = [];
  private nextId = 1;
  activeBlockId: number | null = null;
  dropAnimationTick = 0;
  upAnimationTick = 0;

  back(){
    this.router.navigate(['/']);
  }
  addBlock() {
    let newBlock: Block = { id: this.nextId++, html: '', isNew: true };
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


  exportBlock() {
    if (this.activeBlockId === null) return;

    const block = this.blocks.find(b => b.id === this.activeBlockId);
    if (!block) return;

    if (block.html.length == 0){
      alert('Блок пустой')
      return;
    }

    this.http.post(
      'http://localhost:5200/blocks/export',
      { html: block.html },
      { responseType: 'blob' }
    ).subscribe(blob => {

      const url = window.URL.createObjectURL(blob);

      const a = document.createElement('a');
      a.href = url;
      a.download = 'block.html';
      a.click();

      window.URL.revokeObjectURL(url);
    });
  }

  getTextTemplate(){
    let template: string = '';
    for (let i = 0; i < this.blocks.length; i++) {
      if (this.blocks[i].html.length > 0){
        template += this.blocks[i].html + '\n';
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
      'http://localhost:5200/templates/export',
      { html: template },
      { responseType: 'blob' }
    ).subscribe(blob => {

      const url = window.URL.createObjectURL(blob);

      const a = document.createElement('a');
      a.href = url;
      a.download = 'template.html';
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
      'http://localhost:5200/templates/export',
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
      'http://localhost:5200/templates/export',
      { html: template },
      { responseType: 'blob' }
    ).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);

    });
  }


  async onFileSelected(event?: Event) {
    if (!event) return;

    const input = event.target as HTMLInputElement | null;
    if (!input || !input.files?.length) return;

    const file = input.files[0];
    const text = await file.text();

    this.blocks = [...this.blocks, { id: this.nextId++, html: text, isNew: true }];

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
