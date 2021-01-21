import {
  HttpClient,
  HttpRequest,
  HttpEventType,
  HttpResponse,
} from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { finalize } from 'rxjs/operators';

interface CertificateSubmissionResult {
  fileName: string;
  fileSize: number;
}

@Component({
  selector: 'app-multiple-files-upload',
  templateUrl: './multiple-files-upload.component.html',
  styleUrls: ['./multiple-files-upload.component.css'],
})
export class MultipleFilesUploadComponent implements OnInit {
  studentId = 9998;
  uploadProgress = 0;
  selectedFiles: File[];
  uploading = false;
  errorMsg = '';
  submissionResults: CertificateSubmissionResult[] = [];
  constructor(private readonly httpClient: HttpClient) {}

  ngOnInit() {}

  chooseFile(files: FileList) {
    this.selectedFiles = [];
    this.errorMsg = '';
    this.uploadProgress = 0;
    if (files.length === 0) {
      return;
    }
    for (let i = 0; i < files.length; i++) {
      this.selectedFiles.push(files[i]);
    }
  }

  upload() {
    if (!this.selectedFiles || this.selectedFiles.length === 0) {
      this.errorMsg = 'Please choose a file.';
      return;
    }

    const formData = new FormData();
    this.selectedFiles.forEach((f) => formData.append('certificates', f));

    const req = new HttpRequest(
      'POST',
      `api/students/${this.studentId}/certificates`,
      formData,
      {
        reportProgress: true,
      }
    );
    this.uploading = true;
    this.httpClient
      .request<CertificateSubmissionResult[]>(req)
      .pipe(
        finalize(() => {
          this.uploading = false;
          this.selectedFiles = null;
        })
      )
      .subscribe(
        (event) => {
          if (event.type === HttpEventType.UploadProgress) {
            this.uploadProgress = Math.round(
              (100 * event.loaded) / event.total
            );
          } else if (event instanceof HttpResponse) {
            this.submissionResults = event.body as CertificateSubmissionResult[];
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
