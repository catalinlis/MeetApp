import { Component, inject } from '@angular/core';
import { NavbarComponent } from "../navbar/navbar.component";
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { faImage } from '@fortawesome/free-solid-svg-icons';
import { faArrowRight } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { AccountService } from '../_services/account.service';
import { routes } from '../app.routes';
import { Router } from '@angular/router';

@Component({
  selector: 'app-load-image',
  standalone: true,
  imports: [NavbarComponent, FontAwesomeModule],
  templateUrl: './load-image.component.html',
  styleUrl: './load-image.component.css'
})
export class LoadImageComponent {
  faPlus = faPlus;
  faImage = faImage;
  faArrowRight = faArrowRight;

  previewUrl: string | null = null;
  selectedFile: File | null = null;
  private accountService = inject(AccountService);
  private router = inject(Router);

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

  onSubmit(event: Event): void{
    event.preventDefault();
    console.log("test");
    if(!this.selectedFile){
      console.error('No file selected');
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.accountService.uploadImage(formData).subscribe({
      next: (response) => {
        console.log(response);
        this.accountService.updateRegisterStep(response.registerStep);
        this.accountService.updateProfilePhoto(response.profilePhoto);
        this.router.navigateByUrl("register/choose-interest")
      },
      error: (err) => console.error('Error', err)
    });
  }

}
