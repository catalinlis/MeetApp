import { Component, inject, Inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faClose } from '@fortawesome/free-solid-svg-icons';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SafeUrl } from '@angular/platform-browser';
import { CdkTextareaAutosize, TextFieldModule } from '@angular/cdk/text-field';
import { ViewChild } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PostingService } from '../../_services/posting.service';

@Component({
  selector: 'app-add-photo',
  standalone: true,
  imports: [ MatDialogModule, MatIconModule, FontAwesomeModule, TextFieldModule, MatInputModule, MatSelectModule, MatFormFieldModule, CommonModule, FormsModule],
  templateUrl: './add-photo.component.html',
  styleUrl: './add-photo.component.css'
})
export class AddPhotoComponent {
  faClose = faClose;
  private dialogRef = inject(MatDialogRef);
  private postService = inject(PostingService);
  previewUrl: string | null = null;
  selectedFile: File | null = null;
  textareaContent: string = "";

  constructor(@Inject(MAT_DIALOG_DATA) public data: { userPhoto: SafeUrl, firstname: string, lastname: string }) {}

  @ViewChild('autosize') autosize!: CdkTextareaAutosize;

  closeDialog(): void {
    this.dialogRef.close();
  }

  onFileSelected(event: Event){
    const fileInput = event.target as HTMLInputElement;
    if(fileInput.files && fileInput.files[0]){
      this.selectedFile = fileInput.files[0];

      const reader = new FileReader();
      reader.onload = (e) => {
        this.previewUrl = e.target?.result as string;
      }

      reader.readAsDataURL(this.selectedFile);
    }
  }

  onPost(event: Event){
    const formData = new FormData();
    formData.append('file', this.selectedFile!);
    formData.append('text', this.textareaContent);
    this.closeDialog();
    this.postService.uploadPhoto(formData);
  }
}
