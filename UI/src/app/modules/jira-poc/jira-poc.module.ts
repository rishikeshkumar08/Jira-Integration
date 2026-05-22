import { NgModule } from '@angular/core';
import { JiraPocRoutingModule } from './jira-poc-routing.module';
import { JiraPocService } from './jira-poc.service';

@NgModule({
  imports: [JiraPocRoutingModule],
  providers: [JiraPocService]
})
export class JiraPocModule {}
