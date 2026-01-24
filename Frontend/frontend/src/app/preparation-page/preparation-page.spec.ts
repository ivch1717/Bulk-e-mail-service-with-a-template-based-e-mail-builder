import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PreparationPage } from './preparation-page';

describe('PreparationPage', () => {
  let component: PreparationPage;
  let fixture: ComponentFixture<PreparationPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PreparationPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PreparationPage);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
