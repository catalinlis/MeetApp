import { Routes } from '@angular/router';
import { FirstStyleComponent } from './designs/first-style/first-style.component';
import { RegisterComponent } from './register/register.component';
import { ProfilePageComponent } from './profile/profile-page/profile-page.component';
import { LoadImageComponent } from './load-image/load-image.component';
import { ChooseInterestComponent } from './choose-interest/choose-interest.component';
import { registerGuardGuard } from './_guards/register-guard.guard';
import { loggedInGuard } from './_guards/logged-in.guard';
import { CommunityMembersComponent } from './profile/community/community-members/community-members.component';
import { MemberProfileComponent } from './profile/community/member-profile/member-profile.component';
import { memberPlaceholderResolver } from './_resolvers/member-placeholder.resolver';
import { InterestPageComponent } from './interests/interest-page/interest-page.component';
import { interestPlaceholderResolver } from './_resolvers/interest-placeholder.resolver';
import { InterestsPageComponent } from './interests/interests-page/interests-page.component';
import { MessagesPageComponent } from './messages/messages-page/messages-page.component';

export const routes: Routes = [
    {path: '', component: FirstStyleComponent, canActivate: [loggedInGuard]},
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [loggedInGuard],
        children: [
            {path: 'profile', component: ProfilePageComponent, canActivate: [registerGuardGuard]},
            {path: 'community', component: CommunityMembersComponent},
            {path: 'community/:username', component: MemberProfileComponent, resolve: {member: memberPlaceholderResolver}},
            {path: 'interests', component: InterestsPageComponent},
            {path: 'interests/:interest', component: InterestPageComponent, resolve: {interest: interestPlaceholderResolver}},
            {path: 'messages', component: MessagesPageComponent},
            {path: 'messages/:username', component: MessagesPageComponent, resolve: {username: memberPlaceholderResolver}}
        ]
    },
    {path: 'register', component: RegisterComponent, canActivate: [registerGuardGuard]},
    {path: 'register/image-upload', component: LoadImageComponent, canActivate: [registerGuardGuard]},
    {path: 'register/choose-interest', component: ChooseInterestComponent, canActivate: [registerGuardGuard]},
    {path: '**', component: FirstStyleComponent}
];