import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { FormsModule } from '@angular/forms';
import { SpinnerModule } from '@uiowa/spinner';
import { HttpClientModule } from '@angular/common/http';

import { StudentFormsComponent } from './student-forms/student-forms.component';
import { MultipleFilesUploadComponent } from './multiple-files-upload/multiple-files-upload.component';

@NgModule({
  declarations: [
    AppComponent,
    StudentFormsComponent,
    MultipleFilesUploadComponent,
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpClientModule,
    AppRoutingModule,
    SpinnerModule,
    NgbModule,
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
