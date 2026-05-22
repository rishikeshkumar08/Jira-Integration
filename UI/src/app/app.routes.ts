import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadChildren: () =>
      import('./modules/jira-poc/jira-poc.module').then((m) => m.JiraPocModule)
  },
  { path: '**', redirectTo: '' }
];
