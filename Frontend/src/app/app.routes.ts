import { Routes } from '@angular/router';
import { PreparationPage } from './Pages/preparation-page/preparation-page';
import { HomePage} from './Pages/home-page/home-page';
import {ConstructorPage} from './Pages/constructor-page/constructor-page';
import {StatisticsPage} from './Pages/statistics-page/statistics-page';
import {ConfigPage} from './Pages/config-page/config-page';

export const routes: Routes = [
  { path: 'preparation', component: PreparationPage },
  { path: '', component: HomePage },
  { path: 'constructor', component: ConstructorPage },
  { path: 'statistics', component: StatisticsPage },
  { path: 'config', component: ConfigPage },
];

