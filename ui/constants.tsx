
import { Integration, IntegrationType, Ticket, Agent, Job, Workspace, Workflow, Tool } from './types';
import { MarkerType } from 'reactflow';

export const STATUSES = {
  OPEN: { name: 'Open', color: 'bg-blue-500/20 text-blue-400' },
  IN_PROGRESS: { name: 'In Progress', color: 'bg-yellow-500/20 text-yellow-400' },
  REVIEW: { name: 'Review', color: 'bg-purple-500/20 text-purple-400' },
  DONE: { name: 'Done', color: 'bg-emerald-500/20 text-emerald-400' },
  BLOCKED: { name: 'Blocked', color: 'bg-red-500/20 text-red-400' },
};

export const PRIORITIES = {
  LOW: { name: 'Low', color: 'bg-slate-500/10 text-slate-400 border border-slate-500/20', value: 0 },
  MEDIUM: { name: 'Medium', color: 'bg-blue-500/10 text-blue-400 border border-blue-500/20', value: 1 },
  HIGH: { name: 'High', color: 'bg-orange-500/10 text-orange-400 border border-orange-500/20', value: 2 },
  CRITICAL: { name: 'Critical', color: 'bg-red-500/10 text-red-400 border border-red-500/20', value: 3 },
};

export const MOCK_TOOLS: Tool[] = [
  { 
    id: 't1', 
    name: 'Jira Sync', 
    description: 'Read and update Jira tickets.', 
    category: 'TRACKER', 
    icon: 'layers',
    actions: [
      {
        id: 'jira_read_tickets',
        name: 'Read Tickets',
        description: 'Query and retrieve ticket information',
        dangerLevel: 'safe'
      },
      {
        id: 'jira_read_comments',
        name: 'Read Comments',
        description: 'Access ticket comments and attachments',
        dangerLevel: 'safe'
      },
      {
        id: 'jira_create_ticket',
        name: 'Create Ticket',
        description: 'Create new tickets in Jira',
        dangerLevel: 'moderate'
      },
      {
        id: 'jira_update_ticket',
        name: 'Update Ticket',
        description: 'Modify ticket fields (status, assignee, priority)',
        dangerLevel: 'moderate'
      },
      {
        id: 'jira_add_comment',
        name: 'Add Comment',
        description: 'Post comments on tickets',
        dangerLevel: 'moderate'
      },
      {
        id: 'jira_transition_status',
        name: 'Transition Status',
        description: 'Move tickets through workflow states',
        dangerLevel: 'moderate'
      },
      {
        id: 'jira_delete_ticket',
        name: 'Delete Ticket',
        description: 'Permanently delete tickets',
        dangerLevel: 'destructive'
      }
    ]
  },
  { 
    id: 't2', 
    name: 'GitHub Action', 
    description: 'Commit code and create Pull Requests.', 
    category: 'CODE', 
    icon: 'git-branch',
    actions: [
      {
        id: 'github_read_repo',
        name: 'Read Repository',
        description: 'Access repository files and metadata',
        dangerLevel: 'safe'
      },
      {
        id: 'github_read_issues',
        name: 'Read Issues',
        description: 'Query issues and pull requests',
        dangerLevel: 'safe'
      },
      {
        id: 'github_create_branch',
        name: 'Create Branch',
        description: 'Create new branches in repository',
        dangerLevel: 'moderate'
      },
      {
        id: 'github_commit_code',
        name: 'Commit Code',
        description: 'Push commits to repository',
        dangerLevel: 'moderate'
      },
      {
        id: 'github_create_pr',
        name: 'Create Pull Request',
        description: 'Open new pull requests',
        dangerLevel: 'moderate'
      },
      {
        id: 'github_update_pr',
        name: 'Update Pull Request',
        description: 'Modify pull request details and comments',
        dangerLevel: 'moderate'
      },
      {
        id: 'github_merge_pr',
        name: 'Merge Pull Request',
        description: 'Approve and merge pull requests to main branch',
        dangerLevel: 'destructive'
      },
      {
        id: 'github_delete_branch',
        name: 'Delete Branch',
        description: 'Remove branches from repository',
        dangerLevel: 'destructive'
      }
    ]
  },
  { id: 't3', name: 'Confluence Reader', description: 'Fetch documentation from Confluence.', category: 'KNOWLEDGE', icon: 'database' },
  { id: 't4', name: 'Slack Notify', description: 'Send messages and alerts to Slack channels.', category: 'COMMUNICATION', icon: 'message-square' },
  { id: 't5', name: 'Google Search', description: 'Real-time web search for troubleshooting.', category: 'UTILITY', icon: 'globe' },
  { id: 't6', name: 'Log Analyzer', description: 'Parse and find patterns in server logs.', category: 'UTILITY', icon: 'terminal' },
  { id: 't7', name: 'Linear Updater', description: 'Sync status changes with Linear.', category: 'TRACKER', icon: 'layers' },
  { id: 't8', name: 'Code Executor', description: 'Safely execute Python/Node.js snippets.', category: 'CODE', icon: 'play' },
];

export const MOCK_WORKSPACES: Workspace[] = [
  { id: 'ws-1', name: 'Engineering Alpha' },
  { id: 'ws-2', name: 'Marketing Squad' }
];

export const MOCK_INTEGRATIONS: Integration[] = [
  { 
    id: '1', 
    workspaceId: 'ws-1',
    name: 'Jira Cloud', 
    type: IntegrationType.TRACKER, 
    icon: 'jira', 
    connected: true, 
    lastSync: '10 mins ago',
    url: 'https://company.atlassian.net',
    provider: 'jira',
    username: 'admin@company.com'
  },
  { 
    id: '2', 
    workspaceId: 'ws-1',
    name: 'GitHub', 
    type: IntegrationType.CODE_SOURCE, 
    icon: 'github', 
    connected: true, 
    lastSync: '2 mins ago',
    url: 'https://github.com/nexus-ai',
    provider: 'github',
    username: 'nexus-bot'
  },
  { 
    id: '3', 
    workspaceId: 'ws-1',
    name: 'Confluence', 
    type: IntegrationType.KNOWLEDGE_BASE, 
    icon: 'confluence', 
    connected: false, 
    lastSync: 'Never',
    url: 'https://company.atlassian.net/wiki',
    provider: 'confluence',
    username: 'docs-bot@company.com'
  },
  { 
    id: '4', 
    workspaceId: 'ws-1',
    name: 'Linear', 
    type: IntegrationType.TRACKER, 
    icon: 'linear', 
    connected: true, 
    lastSync: '1 hour ago',
    url: 'https://linear.app/nexus',
    provider: 'linear'
  },
  // Demo integration for second workspace
  { 
    id: '5', 
    workspaceId: 'ws-2',
    name: 'Marketing Asana', 
    type: IntegrationType.TRACKER, 
    icon: 'custom', 
    connected: true, 
    lastSync: '1 day ago',
    url: 'https://asana.com',
    provider: 'custom',
    username: 'marketing-lead@company.com'
  },
];

export const MOCK_AGENTS: Agent[] = [
  { 
    id: 'a1', 
    workspaceId: 'ws-1',
    name: 'DevBot Alpha', 
    role: 'Senior Backend Engineer', 
    status: 'BUSY', 
    capabilities: ['Python', 'Node.js', 'SQL Optimization'], 
    toolActionIds: ['t1', 't2', 't6', 't8'],
    avatarUrl: 'https://picsum.photos/200/200?random=1' 
  },
  { 
    id: 'a2', 
    workspaceId: 'ws-1',
    name: 'QA Sentinel', 
    role: 'Quality Assurance', 
    status: 'IDLE', 
    capabilities: ['E2E Testing', 'Cypress', 'Regression'], 
    toolActionIds: ['t1', 't2', 't5'],
    avatarUrl: 'https://picsum.photos/200/200?random=2' 
  },
  { 
    id: 'a3', 
    workspaceId: 'ws-1',
    name: 'DocuScribe', 
    role: 'Technical Writer', 
    status: 'OFFLINE', 
    capabilities: ['Documentation', 'Markdown', 'API Specs'], 
    toolActionIds: ['t3', 't4'],
    avatarUrl: 'https://picsum.photos/200/200?random=3' 
  },
  { 
    id: 'a4', 
    workspaceId: 'ws-2',
    name: 'CopyBot Ultra', 
    role: 'Copywriter', 
    status: 'IDLE', 
    capabilities: ['SEO', 'Creative Writing'], 
    toolActionIds: ['t4', 't5'],
    avatarUrl: 'https://picsum.photos/200/200?random=4' 
  },
];

export const MOCK_TICKETS: Ticket[] = [
  {
    id: 'T-1024',
    workspaceId: 'ws-1',
    source: 'JIRA',
    internal: false,
    title: 'Fix race condition in payment processing',
    description: 'Users are reporting double charges when clicking the pay button twice rapidly.',
    status: STATUSES.IN_PROGRESS,
    priority: PRIORITIES.CRITICAL,
    satisfaction: 45,
    assignedAgentId: 'a1',
    assignedWorkflowId: 'wf-1',
    comments: [
      { id: 'c1', author: 'Human Manager', content: 'This needs to be fixed ASAP.', timestamp: '2023-10-27 09:00' },
      { id: 'c2', author: 'DevBot Alpha', content: 'Analyzing transaction logs. Identified potential mutex lock failure in payment gateway wrapper.', timestamp: '2023-10-27 09:05' },
      { id: 'c3', author: 'Human Dev', content: 'Check the idempotent keys.', timestamp: '2023-10-27 09:15' }
    ]
  },
  {
    id: 'T-1025',
    workspaceId: 'ws-1',
    source: 'GITHUB',
    internal: false,
    title: 'Update dependencies for security patch',
    description: 'Dependabot flagged lodash version < 4.17.21 as vulnerable.',
    status: STATUSES.OPEN,
    priority: PRIORITIES.HIGH,
    satisfaction: 90,
    comments: [
      { id: 'c4', author: 'Dependabot', content: 'CVE-2023-1234 detected.', timestamp: '2023-10-26 14:00' }
    ]
  },
  {
    id: 'T-1026',
    workspaceId: 'ws-1',
    source: 'LINEAR',
    internal: false,
    title: 'Design new landing page hero section',
    description: 'Needs to match the new brand guidelines provided in Confluence.',
    status: STATUSES.REVIEW,
    priority: PRIORITIES.MEDIUM,
    satisfaction: 85,
    assignedAgentId: 'a2', 
    comments: []
  },
  {
    id: 'T-1027',
    workspaceId: 'ws-1',
    source: 'JIRA',
    internal: false,
    title: 'Production Incident: Memory Leak in Data Ingestion Service causing intermittent 502s',
    description: `We are observing a severe memory leak in the data ingestion microservice (service-ingest-v2). 
    
    Symptoms:
    - Pods are OOMKilled every 2-3 hours.
    - Latency spikes on the /ingest-batch endpoint before crashing.
    - LB returns 502 Bad Gateway during pod restarts.
    
    Initial investigation suggests the buffer allocation in the CSV parser is not being released properly when malformed rows are encountered. We need to run a heap dump analysis, patch the parser, and deploy a hotfix immediately. 
    
    Relevant logs attached in Splunk query: index=prod_logs service=ingest-v2 status=502`,
    status: STATUSES.IN_PROGRESS,
    priority: PRIORITIES.CRITICAL,
    satisfaction: 30,
    assignedAgentId: 'a1',
    comments: [
      { id: 'c10', author: 'SRE OnCall', content: 'Paged at 03:00 AM. Restarted pods manually to mitigate immediate impact. Escalating to backend team.', timestamp: '2023-10-28 03:15' },
      { id: 'c11', author: 'DevBot Alpha', content: 'I have analyzed the last 3 heap dumps. 85% of memory is held by "CSVRowBuffer" objects that are never garbage collected. It seems the error handler in "Parser.ts" keeps a reference to the failed row for logging but never clears it.', timestamp: '2023-10-28 08:00' },
      { id: 'c12', author: 'Tech Lead', content: 'Good catch Alpha. Can you draft a fix?', timestamp: '2023-10-28 08:15' },
      { id: 'c13', author: 'DevBot Alpha', content: 'PR #402 created. I modified the error handler to use a WeakRef and explicitly clear the buffer after logging. Unit tests added for malformed CSVs.', timestamp: '2023-10-28 08:20' },
      { id: 'c14', author: 'Tech Lead', content: 'Reviewing PR #402 now. Logic looks sound. One small nit on the log format, but otherwise good to merge.', timestamp: '2023-10-28 08:30' },
      { id: 'c15', author: 'DevBot Alpha', content: 'Log format updated. Running regression suite.', timestamp: '2023-10-28 08:35' },
      { id: 'c16', author: 'QA Sentinel', content: 'Regression suite passed. Load test with 10GB malformed data completed successfully. Memory usage stable at 400MB.', timestamp: '2023-10-28 09:00' },
      { id: 'c17', author: 'Tech Lead', content: 'Merging and deploying to staging.', timestamp: '2023-10-28 09:10' },
      { id: 'c18', author: 'Customer Support', content: 'Clients are asking for an RFO. When can we expect full resolution?', timestamp: '2023-10-28 09:30' }
    ]
  },
  {
    id: 'M-101',
    workspaceId: 'ws-2',
    source: 'ASANA',
    internal: false,
    title: 'Write blog post for Q4 launch',
    description: 'Topics: AI advancements, new features.',
    status: STATUSES.OPEN,
    priority: PRIORITIES.HIGH,
    satisfaction: 95,
    assignedAgentId: 'a4',
    comments: []
  }
];

export const MOCK_JOBS: Job[] = [
  {
    id: 'j1',
    workspaceId: 'ws-1',
    ticketId: 'T-1024',
    agentId: 'a1',
    workflowId: 'wf-1',
    status: 'IN_PROGRESS',
    progress: 65,
    logs: [
      '[09:01:23] Initializing workspace...',
      '[09:01:25] Pulling latest code from origin/main...',
      '[09:02:10] Running reproduction script...',
      '[09:03:00] Race condition reproduced. Latency: 150ms.',
      '[09:05:00] Applying mutex fix...',
      '[09:06:00] Running unit tests...'
    ],
    startedAt: '2023-10-27 09:01'
  },
  {
    id: 'j2',
    workspaceId: 'ws-1',
    ticketId: 'T-1026',
    agentId: 'a2',
    workflowId: 'wf-2',
    status: 'COMPLETED',
    progress: 100,
    logs: [
      '[14:00:00] Visual regression suite started.',
      '[14:05:00] Comparing snapshots...',
      '[14:10:00] No discrepancies found.'
    ],
    startedAt: '2023-10-26 14:00'
  }
];

export const INITIAL_WORKFLOW_NODES = [
  { 
    id: '1', 
    type: 'input', 
    data: { label: 'Ticket Created' }, 
    position: { x: 250, y: 0 },
    style: { background: '#1e293b', color: '#fff', border: '1px solid #475569' }
  },
  { 
    id: '2', 
    data: { label: 'Write Code (Agent)' }, 
    position: { x: 250, y: 100 },
    style: { background: '#1e293b', color: '#fff', border: '1px solid #6366f1' }
  },
  { 
    id: '3', 
    data: { label: 'Run Tests' }, 
    position: { x: 100, y: 200 },
    style: { background: '#1e293b', color: '#fff', border: '1px solid #10b981' }
  },
  { 
    id: '4', 
    data: { label: 'Manual Review' }, 
    position: { x: 400, y: 200 },
    style: { background: '#1e293b', color: '#fff', border: '1px solid #f59e0b' }
  },
];

export const INITIAL_WORKFLOW_EDGES = [
  { id: 'e1-2', source: '1', target: '2', animated: true, style: { stroke: '#64748b' } },
  { id: 'e2-3', source: '2', target: '3', label: 'Auto', markerEnd: { type: MarkerType.ArrowClosed }, style: { stroke: '#64748b' } },
  { id: 'e2-4', source: '2', target: '4', label: 'Manual', markerEnd: { type: MarkerType.ArrowClosed }, style: { stroke: '#64748b' } },
];

export const MOCK_WORKFLOWS: Workflow[] = [
  {
    id: 'wf-1',
    workspaceId: 'ws-1',
    name: 'Bug Fix Automation',
    nodes: INITIAL_WORKFLOW_NODES,
    edges: INITIAL_WORKFLOW_EDGES
  },
  {
    id: 'wf-2',
    workspaceId: 'ws-1',
    name: 'Feature Implementation',
    nodes: [],
    edges: []
  },
  {
    id: 'wf-3',
    workspaceId: 'ws-1',
    name: 'Security Audit',
    nodes: [],
    edges: []
  }
];
