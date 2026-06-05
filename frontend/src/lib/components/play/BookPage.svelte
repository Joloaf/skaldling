<script lang="ts">
	import { fade } from 'svelte/transition';
	import type { Chunk } from './bookChunks';

	type Props = {
		chunks: Chunk[];
		onTaskToggle: (taskId: string, isCompleted: boolean) => void;
	};

	const { chunks, onTaskToggle }: Props = $props();
</script>

{#each chunks as chunk (chunk.id)}
	{#if chunk.type === 'chapter-heading'}
		<h3 class="chapter-heading">Day {chunk.dayNumber}</h3>
	{:else if chunk.type === 'intro'}
		<p class="passage passage--intro">{chunk.text}</p>
	{:else if chunk.type === 'narrative'}
		<p class="passage">{chunk.text}</p>
	{:else if chunk.type === 'task'}
		{#key chunk.node.id}
			<label class="task" transition:fade={{ duration: 400 }}>
				<input
					type="checkbox"
					checked={chunk.node.adventureTask.isCompleted}
					onchange={(e) =>
						onTaskToggle(chunk.node.adventureTask.id, e.currentTarget.checked)} />
				<span class="task-description">{chunk.node.adventureTask.description}</span>
				<span class="task-points">+{chunk.node.adventureTask.pointValue}</span>
			</label>
		{/key}
	{:else if chunk.type === 'convergence'}
		<p class="passage passage--closing">{chunk.text}</p>
	{/if}
{/each}

<style>
    .chapter-heading {
        margin: 0 0 0.75rem;
        text-align: center;
        color: #5b3a29;
        font-size: 1rem;
        font-variant: small-caps;
        letter-spacing: 0.05em;
    }
    .chapter-heading::before,
    .chapter-heading::after {
        content: '◆';
        margin: 0 0.5rem;
        color: #b39752;
        font-size: 0.8em;
    }

    .passage {
        margin: 0 0 1rem;
        text-indent: 1.5rem;
    }
    .passage--intro,
    .passage--closing {
        font-style: italic;
        text-indent: 0;
        color: #5b3a29;
    }

    .task {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.75rem 1rem;
        margin: 0.5rem 0 1rem;
        background: #fffbef;
        border: 1px solid #e8dca0;
        border-radius: 4px;
        cursor: pointer;
        font-family: system-ui, sans-serif;
        font-size: 0.95rem;
    }
    .task input[type='checkbox'] {
        width: 1.3rem;
        height: 1.3rem;
        cursor: pointer;
    }
    .task-description {
        flex: 1;
    }
    .task-points {
        color: forestgreen;
        font-weight: 600;
    }
</style>