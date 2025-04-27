import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { NavbarComponent } from "../../navbar/navbar.component";
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { images } from '../../constants/interest-resources';
import { InterestService } from '../../_services/interest.service';
import { Interest } from '../../_models/Interest';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faCheck } from '@fortawesome/free-solid-svg-icons';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { AccountService } from '../../_services/account.service';
import { Subscription } from 'rxjs';
import { Location } from '@angular/common';
import { StringProcess } from '../../utils/StringProcess';

@Component({
  selector: 'app-interests-page',
  standalone: true,
  imports: [NavbarComponent, CommonModule, FontAwesomeModule],
  templateUrl: './interest-page.component.html',
  styleUrl: './interest-page.component.css'
})
export class InterestPageComponent implements OnInit{
  faCheck = faCheck;
  faPlus = faPlus;
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private interestService = inject(InterestService);
  private accountService = inject(AccountService);
  private subscriptions: Subscription[] = [];
  private location = inject(Location);
  interestParam: string = '';
  buttons: string[] = ["Posts", "Subscribers"];
  interestData: Interest | null = null;
  images = images;
  username: string | null = null;
  subscribed: boolean = false;
  selectedTab: string = 'About'; // Default selected tab
  tabContent: string = ''; // Content for the selected tab
  loading: boolean = false; // Show loading indicator
  error: string | null = null; // Handle errors

  ngOnInit(): void {
    this.loadData();
  }

  loadData(){
    this.getUsername();
    this.getInterestParameter();
    this.isUserSubscribed();
    this.getInterestData();
    this.getInitialTab();
  }

  getUsername(){
    this.username = this.accountService.currentUser()!.userName;
  }

  getInterestParameter(){
    this.route.params.subscribe((params) => {
      this.interestParam = params['interest']; // Update the username
    });
  }

  getInterestData(){
    this.interestService.getInterestData(this.interestParam).subscribe({
      next: (response) => this.interestData = response,
      error: (err) => console.log(err.error)
    });
  }

  isUserSubscribed(){
    if(this.username)
      this.interestService.isUserSubscribed(this.username, this.interestParam).subscribe({
        next: (response) => {
          this.subscribed = response.subscribed
        }
      });
  }

  addUserInterest(){
    if(this.username)
      this.interestService.addUserInterest(this.username, this.interestParam).subscribe({
        next: _ => this.subscribed = true
      });
  }

  getInitialTab(){
    this.route.queryParams.subscribe((params) => {
      var tab = params['tab'] || 'posts';
      tab = tab.toString();
      this.selectedTab = StringProcess.capitalizeFirstLetter(tab);
      console.log(this.selectedTab);
      this.buttons.indexOf(this.selectedTab);
      this.fetchTabContent(tab);
    });
  }

  fetchTabContent(tabId: string){
    this.loading = true;
    this.error = null;
    
    switch(tabId){
      case 'posts': {
        break;
      }
      case 'subscribers': {
        break;
      }
      default:
        break;
    }
  }

  selectTab(tab: string){
    this.selectedTab = tab;
    var param = tab.charAt(0).toLowerCase() + tab.slice(1);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: param },
      queryParamsHandling: 'merge'
    });

    const url = this.router.createUrlTree([], {
      relativeTo: this.route,
      queryParams: { tab: param }
    });

    this.location.replaceState(url.toString());
    this.fetchTabContent(param);

  }

}
