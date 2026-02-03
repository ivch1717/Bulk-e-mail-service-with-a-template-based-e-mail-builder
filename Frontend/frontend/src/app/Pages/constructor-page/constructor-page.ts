import {ChangeDetectorRef, Component, ElementRef, ViewChild} from '@angular/core';
import {NgForOf} from '@angular/common';
import {HtmlBlock} from '../../Components/html-block/html-block';
import { HttpClient } from '@angular/common/http';

type Block = { id: number, html: string };

@Component({
  selector: 'app-constructor-page',
  imports: [
    HtmlBlock
  ],
  templateUrl: './constructor-page.html',
  styleUrl: './constructor-page.css',
})
export class ConstructorPage {
  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  blocks: Block[] = [];
  private nextId = 1;
  addBlock() {
    this.blocks.push({ id: this.nextId++, html: '' });
  }

  activeBlockId: number | null = null;

  deleteBlock() {
    if (this.activeBlockId === null) return;

    this.blocks = this.blocks.filter(
      b => b.id !== this.activeBlockId
    );

    this.activeBlockId = null;
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


  exportTemplate(){
    let template: string = '';
    for (let i = 0; i < this.blocks.length; i++) {
      if (this.blocks[i].html.length > 0){
        template += this.blocks[i].html + '\n';
      }
    }
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


  async onFileSelected(event?: Event) {
    if (!event) return;

    const input = event.target as HTMLInputElement | null;
    if (!input || !input.files?.length) return;

    const file = input.files[0];
    const text = await file.text();

    this.blocks = [...this.blocks, { id: this.nextId++, html: text }];

    this.cdr.detectChanges();

    input.value = '';
  }

  onBlockHtmlChange(id: string | number, html: string) {
    this.blocks = this.blocks.map(b =>
      b.id === id ? { ...b, html } : b
    );
  }

  trackById = (_: number, b: Block) => b.id;
}
