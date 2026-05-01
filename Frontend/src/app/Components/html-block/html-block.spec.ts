import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HtmlBlock } from './html-block';

describe('HtmlBlock', () => {
  let component: HtmlBlock;
  let fixture: ComponentFixture<HtmlBlock>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HtmlBlock]
    })
    .compileComponents();

    fixture = TestBed.createComponent(HtmlBlock);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
