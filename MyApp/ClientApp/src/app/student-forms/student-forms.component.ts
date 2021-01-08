import { Component, OnInit } from '@angular/core';
import {
  HttpClient,
  HttpRequest,
  HttpEventType,
  HttpResponse,
} from '@angular/common/http';
import { finalize } from 'rxjs/operators';

interface StudentFormSubmissionResult {}

@Component({
  selector: 'app-student-forms',
  templateUrl: './student-forms.component.html',
  styleUrls: ['./student-forms.component.css'],
})
export class StudentFormsComponent implements OnInit {
  studentId = 9998;
  formId = 12;
  uploadProgress = 0;
  selectedFile: File;
  uploading = false;
  errorMsg = '';
  courses: string[] = ['Math', 'Reading'];
  submissionResult: StudentFormSubmissionResult;
  constructor(private readonly httpClient: HttpClient) {}

  ngOnInit() {}

  chooseFile(files: FileList) {
    this.selectedFile = null;
    this.errorMsg = '';
    this.uploadProgress = 0;
    if (files.length === 0) {
      return;
    }
    this.selectedFile = files[0];
  }

  upload() {
    if (!this.selectedFile) {
      this.errorMsg = 'Please choose a file.';
      return;
    }

    const formData = new FormData();
    formData.append('studentFile', this.selectedFile);
    formData.append('formId', this.formId.toString());

    this.courses.forEach((c) => {
      formData.append('courses', c);
    });

    const req = new HttpRequest(
      'POST',
      `api/students/${this.studentId}/forms`,
      formData,
      {
        reportProgress: true,
      }
    );
    this.uploading = true;
    this.httpClient
      .request<StudentFormSubmissionResult>(req)
      .pipe(
        finalize(() => {
          this.uploading = false;
          this.selectedFile = null;
        })
      )
      .subscribe(
        (event) => {
          if (event.type === HttpEventType.UploadProgress) {
            this.uploadProgress = Math.round(
              (100 * event.loaded) / event.total
            );
          } else if (event instanceof HttpResponse) {
            this.submissionResult = event.body;
          }
        },
        (error) => {
          // Here, you can either customize the way you want to catch the errors
          throw error; // or rethrow the error if you have a global error handler
        }
      );
  }

  humanFileSize(bytes: number): string {
    if (Math.abs(bytes) < 1024) {
      return bytes + ' B';
    }
    const units = ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
    let u = -1;
    do {
      bytes /= 1024;
      u++;
    } while (Math.abs(bytes) >= 1024 && u < units.length - 1);
    return bytes.toFixed(1) + ' ' + units[u];
  }
}
