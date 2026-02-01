import { Routes } from '@angular/router';
import { PreparationPage } from './Pages/preparation-page/preparation-page';
import { HomePage} from './Pages/home-page/home-page';
import {ConstructorPage} from './Pages/constructor-page/constructor-page';

export const routes: Routes = [
  { path: 'preparation', component: PreparationPage },
  { path: '', component: HomePage },
  { path: 'constructor', component: ConstructorPage },
];

