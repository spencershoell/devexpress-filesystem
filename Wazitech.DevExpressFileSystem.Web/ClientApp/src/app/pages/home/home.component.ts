import { Component } from '@angular/core';

import RemoteFileSystemProvider from 'devextreme/file_management/remote_provider';

@Component({
  preserveWhitespaces: true,
  styleUrls: ['./home.component.scss'],
  templateUrl: 'home.component.html'
})
export class HomeComponent {
  allowedFileExtensions: string[];

  remoteProvider: RemoteFileSystemProvider;

  constructor() {
    this.allowedFileExtensions = [];
    this.remoteProvider = new RemoteFileSystemProvider({
      endpointUrl: 'https://localhost:7099/api/file-manager-db',
    });
  }
}
