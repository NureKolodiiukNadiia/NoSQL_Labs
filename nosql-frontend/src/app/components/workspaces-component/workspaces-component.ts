import {Component, inject, signal} from '@angular/core';
import {
  CreateWorkspaceRequest,
  FindNearLocationRequest,
  Workspace,
  WorkspaceService
} from '../../services/workspace.service';
import {FormsModule} from '@angular/forms';
import {AgGridAngular} from 'ag-grid-angular';
import {ColDef} from 'ag-grid-community';

@Component({
  selector: 'app-workspaces-component',
  imports: [
    FormsModule,
    AgGridAngular
  ],
  templateUrl: './workspaces-component.html',
  styleUrl: './workspaces-component.css',
})
export class WorkspacesComponent {
  workspacePattern = '';
  deactivateText = '';
  appendWorkspaceId = '';
  appendSuffix = '';
  multiPattern = '';
  multiSuffix = '';
  locationX: number | null = null;
  locationY: number | null = null;
  radiusKm = 10;

  foundWorkspaces = signal<Workspace[]>([]);
  nearWorkspaces = signal<Workspace[]>([]);
  workspaces = signal<Workspace[]>([]);
  workspaceMessage = signal('');
  updatedCount = signal<number | null>(null);
  selectedWorkspace: Workspace | null = null;

  createWorkspaceModel: CreateWorkspaceRequest = {
    name: '',
    spaceType: 0,
    hourlyRate: 0,
    capacity: null,
    amenities: null,
    isActive: true
  };

  private normalizeBool(value: unknown): boolean {
    if (typeof value === 'boolean') {
      return value;
    }

    return false;
  }

  workspaceColumnDefs: ColDef<Workspace>[] = [
    {field: 'id', headerName: 'ID', flex: 2, minWidth: 180,},
    {field: 'name', headerName: 'Name', flex: 2, minWidth: 140},
    {field: 'spaceType', headerName: 'Type', flex: 1},
    {field: 'hourlyRate', headerName: 'Rate', flex: 1},
    {field: 'capacity', headerName: 'Capacity', flex: 1},
    {
      field: 'isActive',
      headerName: 'Active',
      flex: 1,
      valueGetter: (params) =>
        this.normalizeBool(params.data?.isActive) ? 'Active' : 'Inactive'
    }
  ];

  workspaceService = inject(WorkspaceService);

  loadWorkspaces() {
    this.workspaceService.getWorkspaces().subscribe({
      next: (res) => this.workspaces.set(res),
      error: (err) => this.workspaceMessage.set('Failed to load workspaces: ' + err.message)
    });
  }

  createWorkspace() {
    this.workspaceService.createWorkspace(this.createWorkspaceModel).subscribe({
      next: () => {
        this.workspaceMessage.set('Workspace created');
        this.loadWorkspaces();
      },
      error: (err) => this.workspaceMessage.set('Failed to create workspace: ' + err.message)
    });
  }

  onSelectWorkspace(event: any) {
    const selected = event.api.getSelectedRows()[0] as Workspace | undefined;
    this.selectedWorkspace = selected ?? null;
    if (selected) {
      this.createWorkspaceModel = {
        name: selected.name,
        spaceType: selected.spaceType ?? 0,
        hourlyRate: selected.hourlyRate ?? 0,
        capacity: selected.capacity ?? null,
        amenities: selected.amenities ?? null,
        location: selected.location ?? null,
        isActive: this.normalizeBool(selected.isActive)
      };
    }
  }

  updateWorkspace() {
    if (!this.selectedWorkspace) return;
    this.workspaceService.updateWorkspace(this.selectedWorkspace.id, this.createWorkspaceModel).subscribe({
      next: () => {
        this.workspaceMessage.set('Workspace updated');
        this.loadWorkspaces();
      },
      error: (err) => this.workspaceMessage.set('Failed to update workspace: ' + err.message)
    });
  }

  deleteSelectedWorkspace() {
    if (!this.selectedWorkspace) return;
    this.workspaceService.deleteWorkspace(this.selectedWorkspace.id).subscribe({
      next: () => {
        this.workspaceMessage.set('Workspace deleted');
        this.selectedWorkspace = null;
        this.loadWorkspaces();
      },
      error: (err) => this.workspaceMessage.set('Failed to delete workspace: ' + err.message)
    });
  }

  findByNamePattern() {
    if (!this.workspacePattern) {
      return;
    }

    this.workspaceService.findByNamePattern(this.workspacePattern).subscribe({
      next: (res) => this.foundWorkspaces.set(res)
    });
  }

  deactivateMatching() {
    if (!this.deactivateText) {
      return;
    }

    this.workspaceService.deactivateMatchingText(this.deactivateText).subscribe({
      next: () => this.workspaceMessage.set('Workspaces deactivated'),
      error: (err) => this.workspaceMessage.set('Failed: ' + err.message)
    });
  }

  appendTextToWorkspace() {
    if (!this.appendWorkspaceId || !this.appendSuffix) {
      return;
    }

    this.workspaceService.appendTextToWorkspaceName(this.appendWorkspaceId, this.appendSuffix).subscribe({
      next: () => this.workspaceMessage.set('Text appended'),
      error: (err) => this.workspaceMessage.set('Failed: ' + err.message)
    });
  }

  appendTextMultiple() {
    if (!this.multiPattern || !this.multiSuffix) {
      return;
    }

    this.workspaceService.appendTextToMultipleByNamePattern(this.multiPattern, this.multiSuffix).subscribe({
      next: (res) => this.updatedCount.set(res.modifiedCount),
      error: () => this.updatedCount.set(0)
    });
  }

  findNearLocation() {
    if (this.locationX === null || this.locationY === null) return;
    const request: FindNearLocationRequest = {x: this.locationX, y: this.locationY, radiusKm: this.radiusKm};
    this.workspaceService.findNearLocation(request).subscribe({
      next: (res) => {
        if (res.length === 0) {
          this.workspaceMessage.set('No workspaces found near')
        }
        this.nearWorkspaces.set(res)
      }
    });
  }
}
