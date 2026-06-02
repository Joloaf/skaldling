<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import { heroesApi, type HeroDto } from '$lib/api/heroes';
	import {
		adventuresApi,
		type GetAdventureResponse,
		type PlayAdventureTaskDto
	} from '$lib/api/adventures';
	import type { ProblemDetails } from '$lib/api/client';

	type LoadState =
		| { status: 'loading' }
		| { status: 'noActiveAdventure'; hero: HeroDto }
		| { status: 'loaded'; hero: HeroDto; adventure: GetAdventureResponse }
		| { status: 'error'; problem: ProblemDetails };

	type DayState = 'completed' | 'current' | 'locked';

	const heroId = page.params.id!;

	let loadState = $state<LoadState>({ status: 'loading' });
	let isExpanded = $state<Record<string, boolean>>({});
	let lastEarnedScore = $state<number | null>(null);

	$effect(() => {
		loadEverything();
	});

	async function loadEverything() {
		loadState = { status: 'loading' };
		const heroResult = await heroesApi.get(heroId);
		if (!heroResult.ok) {
			loadState = { status: 'error', problem: heroResult.problem };
			return;
		}
		const hero = heroResult.data;
		if (!hero.activeAdventureId) {
			loadState = { status: 'noActiveAdventure', hero };
			return;
		}
		const adventureResult = await adventuresApi.get(hero.activeAdventureId);
		if (!adventureResult.ok) {
			loadState = { status: 'error', problem: adventureResult.problem };
			return;
		}
		loadState = { status: 'loaded', hero, adventure: adventureResult.data };
	}

	const dayStates: DayState[] = $derived.by(() => {
		if (loadState.status !== 'loaded') return [];
		const days = loadState.adventure.days;
		return days.map((day, idx): DayState => {
			const allTasksComplete = day.nodes.every((n) => n.adventureTask.isCompleted);
			if (allTasksComplete) return 'completed';
			const earlierAllComplete = days
				.slice(0, idx)
				.every((d) => d.nodes.every((n) => n.adventureTask.isCompleted));
			return earlierAllComplete ? 'current' : 'locked';
		});
	});

	const adventureScore: number = $derived(
		loadState.status === 'loaded'
			? loadState.adventure.days
				.flatMap((d) => d.nodes)
				.filter((n) => n.adventureTask.isCompleted)
				.reduce((sum, n) => sum + n.adventureTask.pointValue, 0)
			: 0
	);

	const maxAdventureScore: number = $derived(
		loadState.status === 'loaded'
			? loadState.adventure.days
				.flatMap((d) => d.nodes)
				.reduce((sum, n) => sum + n.adventureTask.pointValue, 0)
			: 0
	);

	const allTasksComplete: boolean = $derived(
		loadState.status === 'loaded'
			? loadState.adventure.days
				.flatMap((d) => d.nodes)
				.every((n) => n.adventureTask.isCompleted)
			: false
	);

	function findTask(
		adventure: GetAdventureResponse,
		taskId: string
	): PlayAdventureTaskDto | undefined {
		for (const day of adventure.days) {
			for (const node of day.nodes) {
				if (node.adventureTask.id === taskId) return node.adventureTask;
			}
		}
		return undefined;
	}

	async function handleStart() {
		if (loadState.status !== 'loaded') return;
		const result = await adventuresApi.start(loadState.adventure.id);
		if (result.ok) {
			loadState.adventure.status = result.data.status;
		} else {
			console.error('Start adventure failed:', result.problem);
		}
	}

	async function handleTaskToggle(taskId: string, isCompleted: boolean) {
		if (loadState.status !== 'loaded') return;
		const result = await adventuresApi.updateTaskCompletion(
			loadState.adventure.id,
			taskId,
			{ isCompleted }
		);
		if (result.ok) {
			const task = findTask(loadState.adventure, taskId);
			if (task) task.isCompleted = result.data.isCompleted;
		} else {
			console.error('Task toggle failed:', result.problem);
		}
	}

	async function handleConfirmFinale() {
		if (loadState.status !== 'loaded') return;
		const result = await adventuresApi.confirmFinale(loadState.adventure.id);
		if (result.ok) {
			loadState.adventure.status = result.data.status;
			loadState.hero.achievementPoints = result.data.heroAchievementPoints;
			lastEarnedScore = result.data.earnedScore;
		} else {
			console.error('Confirm finale failed:', result.problem);
		}
	}
</script>

<svelte:head>
	<title>
		{loadState.status === 'loaded' ? loadState.adventure.title : 'Play'} - Skaldling
	</title>
</svelte:head>

{#if loadState.status === 'loading'}
	<p>Loading…</p>
{:else if loadState.status === 'error'}
	<p style="color: crimson">
		Failed to load: {loadState.problem.title ?? 'unknown error'}
	</p>
{:else if loadState.status === 'noActiveAdventure'}
	<h1>{loadState.hero.name}</h1>
	<p>No adventure in flight.</p>
	<p><a href={resolve('/heroes/[id]/new-adventure', { id: heroId })}>Start a new adventure</a></p>
{:else if loadState.status === 'loaded'}
	{@const adventure = loadState.adventure}
	{@const hero = loadState.hero}

	<header style="margin-bottom: 1.5rem; padding-bottom: 1rem; border-bottom: 1px solid #ddd;">
		<h1 style="margin: 0;">{adventure.title}</h1>
		<p style="margin: 0.5rem 0 0; color: #666;">
			{hero.name} · {hero.achievementPoints} lifetime achievement points
			{#if adventure.status === 'Active' || adventure.status === 'Completed'}
				<span style="margin-left: 1rem;">Adventure score: <strong>{adventureScore} / {maxAdventureScore}</strong></span>
			{/if}
		</p>
	</header>

	{#if adventure.status === 'Ready'}
		<section
			style="padding: 1rem; border: 1px solid #ddd; border-radius: 6px; background: #fafafa;">
			<h2 style="margin-top: 0;">Day 1</h2>
			<p style="font-style: italic; color: #555;">
				{adventure.days[0]?.narrativeIntro ?? '(no intro)'}
			</p>
			<button
				onclick={handleStart}
				style="margin-top: 1rem; padding: 0.5rem 1rem; background: forestgreen; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 1rem;">
				Start Adventure
			</button>
		</section>
	{:else if adventure.status === 'Active'}
		{#each adventure.days as day, idx (day.id)}
			{@const state = dayStates[idx]}
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
				{:else if state === 'current' || isExpanded[day.id]}
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
										onchange={(e) =>
											handleTaskToggle(
												node.adventureTask.id,
												e.currentTarget.checked
											)}
									/>
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

					{#if state === 'completed' && isExpanded[day.id]}
						<button
							class="toggle-btn"
							onclick={() => (isExpanded[day.id] = false)}>
							Collapse Day {day.dayNumber}
						</button>
					{/if}
				{:else}
					<button class="toggle-btn" onclick={() => (isExpanded[day.id] = true)}>
						Re-read Day {day.dayNumber}
					</button>
				{/if}
			</section>
		{/each}

		{#if allTasksComplete}
			<section class="finale-cta">
				<h2>The journey ends</h2>
				{#if adventure.finaleReward}
					<p>
						Finale reward: <strong>{adventure.finaleReward}</strong>
					</p>
				{/if}
				<button
					onclick={handleConfirmFinale}
					style="margin-top: 1rem; padding: 0.75rem 1.5rem; background: goldenrod; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 1.1rem;">
					Confirm reward delivered
				</button>
			</section>
		{/if}
	{:else if adventure.status === 'Completed'}
		<section class="completion">
			<h2>🎉 Adventure complete</h2>
			{#if lastEarnedScore !== null}
				<p style="font-size: 1.1rem;">{hero.name} earned <strong>{lastEarnedScore}</strong> achievement points on this adventure.</p>
			{/if}
			<p>Lifetime achievement points: <strong>{hero.achievementPoints}</strong></p>
			<p style="margin-top: 1.5rem;">
				<a href={resolve('/heroes/[id]/details', { id: heroId })}>Back to {hero.name}'s page</a>
			</p>
		</section>

		<section style="margin-top: 2rem;">
			<h3>Re-read the journey</h3>
			{#each adventure.days as day (day.id)}
				<details style="margin: 0.5rem 0;">
					<summary style="cursor: pointer; padding: 0.5rem;">
						Day {day.dayNumber}
					</summary>
					<div style="padding: 0.5rem 1rem;">
						{#if day.narrativeIntro}
							<p style="font-style: italic; color: #555;">{day.narrativeIntro}</p>
						{/if}
						{#each day.nodes as node (node.id)}
							{#if node.narrativeText}<p>{node.narrativeText}</p>{/if}
							<p style="color: #888; font-size: 0.9em;">{node.adventureTask.description} (+{node.adventureTask.pointValue})</p>
						{/each}
						{#if day.narrativeConvergence}
							<p style="font-style: italic; color: #555;">{day.narrativeConvergence}</p>
						{/if}
					</div>
				</details>
			{/each}
		</section>
	{/if}
{/if}

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
    .task-description {
        flex: 1;
    }
    .task-points {
        color: forestgreen;
        font-weight: 600;
    }

    .toggle-btn {
        margin-top: 0.5rem;
        background: none;
        border: 1px solid #ddd;
        padding: 0.3rem 0.6rem;
        border-radius: 4px;
        cursor: pointer;
        font-size: 0.9rem;
        color: #555;
    }

    .finale-cta {
        margin-top: 2rem;
        padding: 1.5rem;
        background: linear-gradient(to bottom, #fffcf0, #fff8e0);
        border: 2px solid goldenrod;
        border-radius: 8px;
        text-align: center;
    }
    .finale-cta h2 {
        margin-top: 0;
        color: #8b6f00;
    }

    .completion {
        padding: 2rem;
        background: linear-gradient(to bottom, #f0fff0, #e0ffe0);
        border: 2px solid forestgreen;
        border-radius: 8px;
        text-align: center;
    }
    .completion h2 {
        margin-top: 0;
        color: forestgreen;
    }
</style>