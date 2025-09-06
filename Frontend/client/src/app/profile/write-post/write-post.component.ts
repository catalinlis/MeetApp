import { Component, Inject, inject } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faClose } from '@fortawesome/free-solid-svg-icons';
import { faArrowLeft } from '@fortawesome/free-solid-svg-icons';
import { faWarning } from '@fortawesome/free-solid-svg-icons';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { SafeUrl } from '@angular/platform-browser';
import { CdkTextareaAutosize, TextFieldModule } from '@angular/cdk/text-field';
import { ViewChild } from '@angular/core';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Interest } from '../../_models/Interest';
import { InterestService } from '../../_services/interest.service';
import { PostingService } from '../../_services/posting.service';

@Component({
  selector: 'app-write-post',
  standalone: true,
  imports: [ MatIconModule, MatDialogModule, FontAwesomeModule, MatFormFieldModule, MatSelectModule, MatInputModule, TextFieldModule, CommonModule, FormsModule ],
  templateUrl: './write-post.component.html',
  styleUrl: './write-post.component.css',
  animations: []
})
export class WritePostComponent{
  faClose = faClose;
  faArrowLeft = faArrowLeft;
  faWarning = faWarning;
  private dialogRef = inject(MatDialogRef);
  private interestService = inject(InterestService);
  private postService = inject(PostingService);
  page: 'post' | 'interest' = 'post';
  interests: {interestKey: string, interestName: string, checked: boolean}[] = [];
  checkedInterests: Interest[] = [];
  checkedInterestsCount: number = 0;
  previewUrl: string | null = null;
  selectedFile: File | null = null;
  textareaContent: string = "";

  constructor(@Inject(MAT_DIALOG_DATA) public data: { userPhoto: SafeUrl, firstname: string, lastname: string }) {}

  @ViewChild('autosize') autosize!: CdkTextareaAutosize;

  closeDialog(): void {
    this.dialogRef.close();
    this.flushCheckedData();
  }

  flushCheckedData(){
    this.checkedInterests = [];
    this.checkedInterestsCount = 0;
    this.interests.forEach( interest => {
      interest.checked = false;
    });
  }

  async goToModalPage(page: 'post' | 'interest'){
    
    if(page === 'interest')
    {
      try{
        if( this.interests.length === 0 )
          await this.getInterests();
      } catch(err) {
        console.error("Failed to load interests");
      }
    }

    this.page = page;
  }

  goBack(){
    this.flushCheckedData();
    this.goToModalPage('post');
  }

  selectInterests(){
    this.setInterestsChecked();
    this.goToModalPage('post');
  }

  getInterests(): Promise<any>{
    return new Promise((resolve, reject) => {
        this.interestService.getInterests().subscribe({
          next: (response) => {
            var interestsInfo = response;

            for(const interestInfo of interestsInfo){
              var interest = { interestKey: interestInfo.interestKey, interestName: interestInfo.interestName, checked: false  };
              this.interests.push(interest);
            }

            resolve(response);
          },
          error: (err) => {
            console.log(err.error);
            reject(err);
          }
        })
      });
  }

  countCheckedInterests(event: Event){
    const checkbox = event.target as HTMLInputElement;

    if(checkbox.checked)
      this.checkedInterestsCount++;
    else
      this.checkedInterestsCount--;
    
  }

  setInterestsChecked() : void{
    this.checkedInterests = [];

    if(this.checkedInterestsCount <= 3)
      for(const interest of this.interests){
        if(interest.checked === true){
          var checkedInterest: Interest = {
            interestKey: interest.interestKey,
            interestName: interest.interestName
          };
          this.checkedInterests.push(checkedInterest);
        }
      }
  }

  uncheckInterest(removeInterest: Interest){
    this.checkedInterests = this.checkedInterests.filter(interest => interest.interestKey !== removeInterest.interestKey);
    this.interests.forEach( interest => {
      if(interest.interestKey === removeInterest.interestKey)
        interest.checked = false;
    });
    this.checkedInterestsCount--;
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

  onPost(event: Event): void{
    event.preventDefault();
    
    const formData =  new FormData();
    if(this.selectedFile !== null) 
      formData.append('file', this.selectedFile!)
    else
      formData.append('file', '');
    formData.append('text', this.textareaContent);
    formData.append('interestKeys', '');
    formData.forEach(item => {
    })

    this.checkedInterests.forEach(interest => { 
      formData.append('interestKeys', interest.interestKey);
    });

    this.closeDialog();
    this.postService.uploadPost(formData);

  }
}
