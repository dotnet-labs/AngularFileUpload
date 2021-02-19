import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FileDownloadComponent } from './file-download/file-download.component';
import { MultipleFilesUploadComponent } from './multiple-files-upload/multiple-files-upload.component';
import { StudentFormsComponent } from './student-forms/student-forms.component';

const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'single-file',
  },
  {
    path: 'single-file',
    component: StudentFormsComponent,
  },
  {
    path: 'multi-files',
    component: MultipleFilesUploadComponent,
  },
  {
    path: 'file-download',
    component: FileDownloadComponent,
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
