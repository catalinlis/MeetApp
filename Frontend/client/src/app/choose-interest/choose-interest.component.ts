import { Component, inject, OnInit } from '@angular/core';
import { NavbarComponent } from "../navbar/navbar.component";
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { AccountService } from '../_services/account.service';
import { Interest } from '../_models/Interest';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-choose-interest',
  standalone: true,
  imports: [NavbarComponent, FontAwesomeModule, FormsModule],
  templateUrl: './choose-interest.component.html',
  styleUrl: './choose-interest.component.css'
})
export class ChooseInterestComponent implements OnInit{
  faPlus=faPlus;
  interestsInfo: Interest[] = [];
  interestsData: {interestKey: string, interestName: string, checked: boolean}[] = [];
  private accountService = inject(AccountService);
  router = inject(Router);

  async ngOnInit(): Promise<void> {
    this.getInterests();
  }

  getInterests(): void {
    this.accountService.getInterests().subscribe({
      next: (resp) => {
        this.interestsInfo = resp;

        for(const interestInfo of this.interestsInfo){
          var interest = { interestKey: interestInfo.interestKey, interestName: interestInfo.interestName, checked: false  };
          this.interestsData.push(interest);
        }
        console.log(this.interestsData);
      },
      error: (err) => console.log(err.error)
    });
  }

  getCheckedInterests() : void{
    var interests = Array<Interest>();

    for(const interest of this.interestsData){
      if(interest.checked === true){
        var checkedInterest: Interest = {
          interestKey: interest.interestKey,
          interestName: interest.interestName
        };
        interests.push(checkedInterest);
      }
    }

    this.accountService.setInterests(interests).subscribe({
      next: (response) => {
        console.log(response);
        this.accountService.updateRegisterStep(response.registerStep);
        this.router.navigateByUrl("/profile");
      },
      error: (error) => console.log(error.error)
    });
  }

}
