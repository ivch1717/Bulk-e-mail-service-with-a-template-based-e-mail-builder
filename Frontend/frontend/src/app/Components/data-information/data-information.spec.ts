import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DataInformation } from './data-information';

describe('DataInformation', () => {
  let component: DataInformation;
  let fixture: ComponentFixture<DataInformation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DataInformation]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DataInformation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
