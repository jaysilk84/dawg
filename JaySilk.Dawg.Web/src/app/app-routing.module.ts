import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DawgChartComponent } from './dawg-chart/dawg-chart.component';
import { ScrabbleComponent } from './scrabble/scrabble.component';
import { AppComponent } from './app.component';

const routes: Routes = [
  {path: 'graph', component: DawgChartComponent },
  {path: 'scrabble', component: ScrabbleComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
