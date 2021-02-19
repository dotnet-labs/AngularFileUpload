import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-file-download',
  templateUrl: './file-download.component.html',
  styleUrls: ['./file-download.component.css'],
})
export class FileDownloadComponent implements OnInit {
  link = `api/students/files/1`;
  constructor() {}

  ngOnInit(): void {}
}
