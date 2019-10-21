import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DawgChartComponent } from './dawg-chart.component';

describe('DawgChartComponent', () => {
  let component: DawgChartComponent;
  let fixture: ComponentFixture<DawgChartComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DawgChartComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DawgChartComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
