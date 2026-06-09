<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import { heroesApi, type HeroDto } from '$lib/api/heroes';
	import {
		adventuresApi,
		type ThemeDto,
		type CreateAdventureCommand,
		type NarrativeStyle,
		type GenerateStoryResponse
	} from '$lib/api/adventures';
	import type { ProblemDetails } from '$lib/api/client';
	import StoryLoader from '$lib/components/StoryLoader.svelte'
	import Icon from '$lib/components/Icon.svelte';

	type PageState =
		| { status: 'loading' }
		| { status: 'active'; hero: HeroDto }
		| { status: 'loaded'; hero: HeroDto; themes: ThemeDto[] }
		| { status: 'error'; problem: ProblemDetails };

	type SubmitState =
		| { status: 'idle' }
		| { status: 'submitting' }
		| { status: 'success'; adventureId: string; title: string }
		| { status: 'error'; problem: ProblemDetails };

	type GenerateState =
		| { status: 'idle' }
		| { status: 'generating' }
		| { status: 'generated'; storyStatus: GenerateStoryResponse['status'] }
		| { status: 'error'; problem: ProblemDetails };

	let pageState: PageState = $state({ status: 'loading' });
	let submitState: SubmitState = $state({ status: 'idle' });
	let generateState: GenerateState = $state({ status: 'idle' });

	const heroId = page.params.id!;

	const POINTS_BY_DIFFICULTY = { Easy: 1, Medium: 2, Hard: 3 } as const;

	type TaskForm = {
		description: string;
		difficulty: 'Easy' | 'Medium' | 'Hard';
		pointValue: number;
	};
	type DayForm = { dayNumber: number; tasks: TaskForm[] };

	let title = $state('');
	let themeId = $state('');
	let tone: 'Cozy' | 'Epic' | 'Mysterious' | 'Comedic' | 'Spooky' = $state('Cozy');
	let narrativeStyle: NarrativeStyle = $state('TaskIntegrated');
	let moral = $state('');
	let finaleReward = $state('');
	let days: DayForm[] = $state(
		Array.from({ length: 5 }, (_, i) => ({
			dayNumber: i + 1,
			tasks: [{ description: '', difficulty: 'Medium', pointValue: 2 }]
		}))
	);

	$effect(() => {
		loadEverything();
	});

	async function loadEverything() {
		pageState = { status: 'loading' };
		const [heroResult, themesResult] = await Promise.all([
			heroesApi.get(heroId),
			adventuresApi.listThemes()
		]);
		if (!heroResult.ok) {
			pageState = { status: 'error', problem: heroResult.problem };
			return;
		}
		if (!themesResult.ok) {
			pageState = { status: 'error', problem: themesResult.problem };
			return;
		}
		const hero = heroResult.data;

		if (hero.activeAdventureId) {
			pageState = { status: 'active', hero };
			return;
		}

		const themes = themesResult.data.themes;
		title = `${hero.name}'s ${themes[0]?.name ?? ''} Adventure`;
		themeId = themes.find((t) => t.isDefault)?.id ?? themes[0]?.id ?? '';

		pageState = { status: 'loaded', hero, themes };
	}

	function addTask(dayIndex: number) {
		if (days[dayIndex].tasks.length >= 3) return;
		days[dayIndex].tasks = [
			...days[dayIndex].tasks,
			{ description: '', difficulty: 'Medium', pointValue: 2 }
		];
	}

	function removeTask(dayIndex: number, taskIndex: number) {
		if (days[dayIndex].tasks.length <= 1) return;
		days[dayIndex].tasks = days[dayIndex].tasks.filter((_, i) => i !== taskIndex);
	}

	function setDifficulty(
		dayIndex: number,
		taskIndex: number,
		difficulty: 'Easy' | 'Medium' | 'Hard'
	) {
		const task = days[dayIndex].tasks[taskIndex];
		task.difficulty = difficulty;
		task.pointValue = POINTS_BY_DIFFICULTY[difficulty];
	}

	async function createAdventure() {
		submitState = { status: 'submitting' };
		const command: CreateAdventureCommand = {
			heroId,
			themeId,
			title,
			tone,
			narrativeStyle,
			moral: moral.trim() === '' ? null : moral,
			finaleReward: finaleReward.trim() === '' ? null : finaleReward,
			days
		};
		const result = await adventuresApi.create(command);
		if (result.ok) {
			submitState = {
				status: 'success',
				adventureId: result.data.id,
				title: result.data.title
			};
		} else {
			submitState = { status: 'error', problem: result.problem };
		}
	}

	async function generateStory(adventureId: string) {
		generateState = { status: 'generating' };
		const result = await adventuresApi.generate(adventureId);
		if (result.ok) {
			generateState = { status: 'generated', storyStatus: result.data.status };
		} else {
			generateState = { status: 'error', problem: result.problem };
		}
	}
</script>

<h1>New adventure</h1>

{#if pageState.status === 'loading'}
	<p>Loading…</p>
{:else if pageState.status === 'error'}
	<p class="msg-error">Failed to load: {pageState.problem.title ?? 'unknown error'}</p>
{:else if pageState.status === 'active'}
	<p>
		<strong>{pageState.hero.name}</strong> is already on an adventure:
		<em>{pageState.hero.activeAdventureTitle}</em>. Finish or abandon it before starting a new one.
	</p>
{:else if pageState.status === 'loaded'}
	{#if submitState.status === 'success'}
		<p class="msg-success">Adventure "<strong>{submitState.title}</strong>" created!</p>
		{#if generateState.status === 'idle'}
			<div style="margin-top: 1rem;">
				<button onclick={() => generateStory(submitState.status === 'success' ? submitState.adventureId : '')}
				        style="padding: 0.5rem 1rem;">
					<Icon name="create" />Generate story
				</button>
				<small style="display: block; margin-top: 0.5rem;">The skald might need a minute or two to compose the story.</small>
			</div>
		{:else if generateState.status === 'generating'}
			<StoryLoader />
		{:else if generateState.status === 'generated'}
			<p class="msg-success" style="margin-top: 1rem;">Story ready! Status: <strong>{generateState.storyStatus}</strong>.</p>
		{:else if generateState.status === 'error'}
			<div class="msg-error" style="margin-top: 1rem;">
				<p>Generation failed: {generateState.problem.title ?? 'unknown error'}.</p>
				<button onclick={() => generateStory(submitState.status === 'success' ? submitState.adventureId : '')}
				        style="margin-top: 0.5rem; padding: 0.4rem 0.8rem;">Try again</button>
			</div>
		{/if}
		<p style="margin-top: 1.5rem;"><a href={resolve('/heroes')}>Back to dashboard</a></p>
	{:else}
		<p>Designing an adventure for <strong>{pageState.hero.name}</strong>.</p>
		<form onsubmit={(e) => { e.preventDefault(); createAdventure(); }}>

			<!-- Title -->
			<div style="margin-bottom: 1rem;">
				<label style="display: block;">
					<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Adventure title</span>
					<input
						type="text"
						bind:value={title}
						required
						maxlength="200"
						style="width: 100%; max-width: 500px; padding: 0.4rem 0.5rem;"
					/>
				</label>
			</div>

			<!-- Theme -->
			<div style="margin-bottom: 1rem;">
				<label style="display: block;">
					<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Theme</span>
					<select bind:value={themeId} style="padding: 0.35rem 0.5rem;">
						{#each pageState.themes as theme (theme.id)}
							<option value={theme.id}>{theme.name}</option>
						{/each}
					</select>
				</label>
				{#if pageState.themes.find((t) => t.id === themeId)}
					<small style="display: block; margin-top: 0.25rem; max-width: 600px;">
						{pageState.themes.find((t) => t.id === themeId)?.description}
					</small>
				{/if}
			</div>

			<!-- Tone -->
			<div style="margin-bottom: 1rem;">
				<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Tone</span>
				<div class="toggle-group">
					{#each ['Cozy', 'Epic', 'Mysterious', 'Comedic', 'Spooky'] as const as toneOption (toneOption)}
						<label class="toggle">
							<input type="radio" bind:group={tone} value={toneOption} />
							{toneOption}
						</label>
					{/each}
				</div>
			</div>

			<!-- Narrative style -->
			<div style="margin-bottom: 1rem;">
				<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Story style</span>
				<label class="toggle toggle--block">
					<input type="radio" bind:group={narrativeStyle} value="TaskIntegrated" />
					<span><strong>Integrated</strong> - The story is woven around the real life activities, where the hero and child encounter challenges as one. <em>(Recommended.)</em></span>
				</label>
				<label class="toggle toggle--block">
					<input type="radio" bind:group={narrativeStyle} value="TaskIndependent" />
					<span><strong>Independent</strong> - The narrative is its own adventure where the real life activities are not tied to the actual story.</span>
				</label>
			</div>

			<!-- Moral -->
			<div style="margin-bottom: 1rem;">
				<label style="display: block;">
					<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Moral <em style="font-weight: normal; color: var(--text-muted);">(optional)</em></span>
					<textarea
						bind:value={moral}
						maxlength="500"
						rows="2"
						style="width: 100%; max-width: 500px; padding: 0.4rem 0.5rem; resize: vertical;"
					></textarea>
				</label>
				<small style="display: block; margin-top: 0.25rem; max-width: 500px;">
					Fill in a sentence or two if you want the adventure to gently teach something specific (kindness, perseverance, fairness, …). Leave blank to let the story unfold without a guiding moral.
				</small>
			</div>

			<!-- Finale reward -->
			<div style="margin-bottom: 1.5rem;">
				<label style="display: block;">
					<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Finale reward <em style="font-weight: normal; color: var(--text-muted);">(optional)</em></span>
					<input
						type="text"
						bind:value={finaleReward}
						maxlength="200"
						placeholder="lördagsgodis"
						style="width: 100%; max-width: 500px; padding: 0.4rem 0.5rem;"
					/>
				</label>
				<small style="display: block; margin-top: 0.25rem; max-width: 500px;">
					A treat of your choice to be rewarded for completing the adventure. Leave blank if you'd rather skip the reward step.
				</small>
			</div>

			<!-- Days -->
			<h2 style="margin-top: 2rem;">Daily real world challenges</h2>
			<p style="color: var(--text-muted); font-size: 0.9rem; margin-top: 0;">
				Each day has 1–3 challenges. Difficulty sets the default point value, but feel free to adjust the value where needed.
			</p>

			{#each days as day, dayIndex (day.dayNumber)}
				<section class="day-card" style="padding: 0.75rem 1rem; margin-bottom: 0.75rem;">
					<h3 style="margin: 0 0 0.5rem; font-size: 1rem;">Day {day.dayNumber}</h3>

					{#each day.tasks as task, taskIndex (taskIndex)}
						<div style="display: grid; grid-template-columns: 1fr auto auto auto; gap: 0.5rem; align-items: center; margin-bottom: 0.5rem;">
							<input
								type="text"
								bind:value={task.description}
								required
								maxlength="500"
								placeholder="Challenge description"
								style="padding: 0.3rem 0.5rem;"
							/>

							<div class="difficulty-group">
								{#each ['Easy', 'Medium', 'Hard'] as const as diff (diff)}
									<label class="difficulty-toggle">
										<input
											type="radio"
											name={`difficulty-${day.dayNumber}-${taskIndex}`}
											checked={task.difficulty === diff}
											onchange={() => setDifficulty(dayIndex, taskIndex, diff)}
										/>
										{diff}
									</label>
								{/each}
							</div>

							<input
								type="number"
								bind:value={task.pointValue}
								min="1"
								max="1000"
								style="width: 70px; padding: 0.3rem 0.5rem; text-align: right;"
							/>

							<button
								type="button"
								class="remove-task"
								onclick={() => removeTask(dayIndex, taskIndex)}
								disabled={day.tasks.length <= 1}
								title={day.tasks.length <= 1 ? 'Each day must have at least one challenge' : 'Remove challenge'}
							><Icon name="close" size={16} /></button>
						</div>
					{/each}

					<button
						type="button"
						onclick={() => addTask(dayIndex)}
						disabled={day.tasks.length >= 3}
						style="margin-top: 0.25rem; padding: 0.25rem 0.75rem;"
					>
						{day.tasks.length >= 3 ? 'Maximum 3 challenges per day' : '+ Add challenge'}
					</button>
				</section>
			{/each}

			<!-- Submit -->
			<div style="margin-top: 2rem;">
				<button
					type="submit"
					disabled={submitState.status === 'submitting' || !title.trim() || !themeId}
					style="padding: 0.5rem 1rem;"
				>
					{submitState.status === 'submitting' ? 'Creating…' : 'Create adventure'}
				</button>
			</div>
		</form>

		{#if submitState.status === 'error'}
			<div class="msg-error" style="margin-top: 1rem;">
				<p>Failed to create: {submitState.problem.title ?? 'unknown error'}</p>
				{#if submitState.problem.errors}
					<ul>
						{#each Object.entries(submitState.problem.errors) as [field, messages] (field)}
							<li><strong>{field}:</strong> {messages.join(', ')}</li>
						{/each}
					</ul>
				{/if}
			</div>
		{/if}
	{/if}
{/if}

<style>
    small { color: var(--text-muted); }

    input[type='text'],
    input[type='number'],
    select,
    textarea {
        background: #251c3a;
        color: var(--text);
        border: 1px solid var(--border);
        border-radius: 8px;
        font: inherit;
    }
    input[type='text']:focus,
    input[type='number']:focus,
    select:focus,
    textarea:focus {
        outline: none;
        border-color: var(--accent);
        box-shadow: 0 0 0 3px color-mix(in srgb, var(--accent) 28%, transparent);
    }
    input::placeholder, textarea::placeholder { color: var(--text-muted); }
    option { background: #251c3a; color: var(--text); }

    button {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        border: 1px solid var(--border);
        border-radius: 10px;
        background: var(--surface-alt);
        color: var(--text);
        font: inherit;
        font-weight: 700;
        cursor: pointer;
    }
    button:hover:not(:disabled),
    button:focus-visible:not(:disabled) {
        border-color: var(--accent);
        color: var(--accent);
        box-shadow: 0 0 0 1px var(--accent), 0 6px 24px color-mix(in srgb, var(--accent) 38%, transparent);
        outline: none;
    }
    button:active:not(:disabled) { transform: scale(0.98); }
    button:disabled { opacity: 0.5; cursor: default; }
    button.remove-task { width: 36px; height: 36px; padding: 0; justify-content: center; }

    .toggle-group, .difficulty-group { display: flex; flex-wrap: wrap; gap: 0.5rem; }
    .toggle, .difficulty-toggle {
        display: inline-flex;
        align-items: center;
        gap: 0.4rem;
        border: 1px solid var(--border);
        border-radius: 10px;
        background: var(--surface-alt);
        color: var(--text);
        padding: 0.4rem 0.8rem;
        cursor: pointer;
        transition: border-color 0.15s, background 0.15s, color 0.15s;
    }
    .difficulty-toggle { font-size: 0.85rem; padding: 0.25rem 0.6rem; }
    .toggle:hover, .difficulty-toggle:hover { border-color: var(--accent); color: var(--accent); }
    .toggle:has(input:checked), .difficulty-toggle:has(input:checked) {
        border-color: var(--accent);
        background: var(--accent-tint);
        color: var(--accent);
    }
    .toggle input[type='radio'], .difficulty-toggle input[type='radio'] {
        position: absolute; opacity: 0; pointer-events: none;
    }
    .toggle--block { display: block; max-width: 600px; margin-bottom: 0.5rem; }
    .toggle em { color: var(--text-muted); }

    .day-card { background: var(--surface-alt); border: 1px solid var(--border); border-radius: 12px; }

    .msg-success { color: var(--accent); }
    .msg-error { color: var(--accent-warm); }
</style>
