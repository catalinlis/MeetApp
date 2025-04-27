import { CanActivateFn, Router } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { inject } from '@angular/core';

export const loggedInGuard: CanActivateFn = (route, state) => {
  const accountService = inject(AccountService);
  const router = inject(Router);
  
  if(accountService.currentUser()){
    if(state.url === '/'){
      router.navigate(['/profile']);
      return false;
    }
    
    return true;
  }

  if(state.url === '/' || state.url == '/register')
    return true;
  else{
    router.navigate(['/']);
    return false;
  }
};
