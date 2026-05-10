import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type HtmlFileType = 'Block' | 'Template';

export interface FileSummary {
  id: string;
  name: string;
  createdAt: string;
  type: HtmlFileType;
}

export interface GetAllFilesResponse {
  files: FileSummary[];
}

export interface AddFileResponse {
  fileId: string;
}

export interface FileDetails {
  id: string;
  name: string;
  content: string;
  createdAt: string;
  type: HtmlFileType;
}

@Injectable({
  providedIn: 'root'
})
export class FilesService {
  constructor(private http: HttpClient) {}

  getAllFiles(): Observable<GetAllFilesResponse> {
    return this.http.get<GetAllFilesResponse>('/files');
  }

  addFile(name: string, content: string, type: HtmlFileType): Observable<AddFileResponse> {
    return this.http.post<AddFileResponse>('/files', {
      name,
      content,
      createdAt: new Date().toISOString(),
      type
    });
  }

  deleteFile(id: string): Observable<unknown> {
    return this.http.delete(`/files/${id}`);
  }

  getFileById(id: string): Observable<FileDetails> {
    return this.http.get<FileDetails>(`/files/${id}`);
  }
}
