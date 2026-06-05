<script lang="ts">
	import type { PlayDayDto } from '$lib/api/adventures';
	import type { ProgressState } from './types';

	type Props = {
		day: PlayDayDto;
		state: ProgressState;
		readOnly?: boolean;
		onTaskToggle: (taskId: string, isCompleted: boolean) => void;
	};

	const { day, state, readOnly = false, onTaskToggle }: Props = $props();
</script>

<section class="day day--{state}">
	<header class="day-header">
		<h2>Day {day.dayNumber}</h2>
		{#if state === 'completed'}
			<span class="day-badge day-badge--completed">Complete</span>
		{:else if state === 'current'}
			<span class="day-badge day-badge--current">In progress</span>
		{:else}
			<span class="day-badge day-badge--locked">Locked</span>
		{/if}
	</header>

	{#if state === 'locked'}
		<p class="locked-hint">
			Complete Day {day.dayNumber - 1} to unlock.
		</p>
	{:else}
		{#if day.narrativeIntro}
			<p class="day-intro">{day.narrativeIntro}</p>
		{/if}

		<ol class="task-list">
			{#each day.nodes as node (node.id)}
				<li class="node">
					{#if node.narrativeText}
						<p class="node-narrative">{node.narrativeText}</p>
					{/if}
					<label class="task">
						<input
							type="checkbox"
							checked={node.adventureTask.isCompleted}
							disabled={readOnly}
							onchange={(e) =>
								onTaskToggle(node.adventureTask.id, e.currentTarget.checked)} />
						<span class="task-description">
							{node.adventureTask.description}
						</span>
						<span class="task-points">+{node.adventureTask.pointValue}</span>
					</label>
				</li>
			{/each}
		</ol>

		{#if state === 'completed' && day.narrativeConvergence}
			<p class="day-convergence">{day.narrativeConvergence}</p>
		{/if}
	{/if}
</section>

<style>
    .day {
        margin: 1rem 0;
        padding: 1rem;
        border: 1px solid #ddd;
        border-radius: 6px;
    }
    .day--completed {
        background: #f5fff5;
        border-color: #cde7c4;
    }
    .day--current {
        background: #fffef0;
        border-color: #e8dca0;
    }
    .day--locked {
        background: #f5f5f5;
        opacity: 0.6;
    }

    .day-header {
        display: flex;
        justify-content: space-between;
        align-items: baseline;
        margin-bottom: 0.5rem;
    }
    .day-header h2 {
        margin: 0;
    }

    .day-badge {
        font-size: 0.85rem;
        padding: 0.2rem 0.5rem;
        border-radius: 4px;
        font-weight: 500;
    }
    .day-badge--completed {
        color: forestgreen;
    }
    .day-badge--current {
        color: #8b6f00;
    }
    .day-badge--locked {
        color: #999;
    }

    .day-intro,
    .day-convergence {
        font-style: italic;
        color: #555;
    }

    .locked-hint {
        color: #888;
        font-size: 0.9rem;
        margin: 0;
    }

    .task-list {
        list-style: none;
        padding: 0;
        margin: 1rem 0;
    }
    .node {
        margin: 1rem 0;
        padding: 0.5rem 0.5rem 0.5rem 1rem;
        border-left: 2px solid #ddd;
    }
    .node-narrative {
        margin: 0 0 0.5rem;
    }

    .task {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        cursor: pointer;
    }
    .task input[type='checkbox'] {
        width: 1.2rem;
        height: 1.2rem;
        cursor: pointer;
    }
    .task input[type='checkbox']:disabled {
        cursor: default;
    }
    .task-description {
        flex: 1;
    }
    .task-points {
        color: forestgreen;
        font-weight: 600;
    }
</style>