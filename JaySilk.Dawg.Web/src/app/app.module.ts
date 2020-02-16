import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DawgChartComponent } from './dawg-chart/dawg-chart.component';
import { ScrabbleComponent } from './scrabble/scrabble.component';
import { GridComponent } from './scrabble/grid/grid.component';
import { NgMatrixDirective } from './ng-matrix.directive';

@NgModule({
  declarations: [
    AppComponent,
    DawgChartComponent,
    ScrabbleComponent,
    GridComponent,
    NgMatrixDirective
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
