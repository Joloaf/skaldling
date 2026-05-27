<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import { heroesApi, type HeroDto } from '$lib/api/heroes';
	import {
		adventuresApi,
		type ThemeDto,
		type CreateAdventureCommand,
		type NarrativeStyle
	} from '$lib/api/adventures';
	import type { ProblemDetails } from '$lib/api/client';

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

	let pageState: PageState = $state({ status: 'loading' });
	let submitState: SubmitState = $state({ status: 'idle' });

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

	async function submit() {
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
</script>

<h1>New adventure</h1>

{#if pageState.status === 'loading'}
	<p>Loading…</p>
{:else if pageState.status === 'error'}
	<p style="color: crimson">Failed to load: {pageState.problem.title ?? 'unknown error'}</p>
{:else if pageState.status === 'active'}
	<p>
		<strong>{pageState.hero.name}</strong> is already on an adventure:
		<em>{pageState.hero.activeAdventureTitle}</em>. Finish or abandon it before starting a new one.
	</p>
{:else if pageState.status === 'loaded'}
	{#if submitState.status === 'success'}
		<p style="color: forestgreen;">Adventure "<strong>{submitState.title}</strong>" created!</p>
		<p style="margin-top: 1rem;"><a href={resolve('/heroes')}>Back to dashboard</a></p>
	{:else}
		<p>Designing an adventure for <strong>{pageState.hero.name}</strong>.</p>
		<form onsubmit={(e) => { e.preventDefault(); submit(); }}>

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
					<small style="display: block; color: #666; margin-top: 0.25rem; max-width: 600px;">
						{pageState.themes.find((t) => t.id === themeId)?.description}
					</small>
				{/if}
			</div>

			<!-- Tone -->
			<div style="margin-bottom: 1rem;">
				<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Tone</span>
				{#each ['Cozy', 'Epic', 'Mysterious', 'Comedic', 'Spooky'] as const as toneOption (toneOption)}
					<label style="display: inline-block; margin-right: 1rem;">
						<input type="radio" bind:group={tone} value={toneOption} />
						{toneOption}
					</label>
				{/each}
			</div>

			<!-- Narrative style -->
			<div style="margin-bottom: 1rem;">
				<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Story style</span>
				<label style="display: block; margin-bottom: 0.25rem;">
					<input type="radio" bind:group={narrativeStyle} value="TaskIntegrated" />
					<strong>Integrated</strong> - The story is woven around the real life activities, where the hero and child encounter challenges as one. <em>(Recommended.)</em>
				</label>
				<label style="display: block;">
					<input type="radio" bind:group={narrativeStyle} value="TaskIndependent" />
					<strong>Independent</strong> - The narrative is its own adventure where the real life activities are not tied to the actual story.
				</label>
			</div>

			<!-- Moral -->
			<div style="margin-bottom: 1rem;">
				<label style="display: block;">
					<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Moral <em style="font-weight: normal; color: #666;">(optional)</em></span>
					<textarea
						bind:value={moral}
						maxlength="500"
						rows="2"
						style="width: 100%; max-width: 500px; padding: 0.4rem 0.5rem; resize: vertical;"
					></textarea>
				</label>
				<small style="display: block; color: #666; margin-top: 0.25rem; max-width: 500px;">
					Fill in a sentence or two if you want the adventure to gently teach something specific (kindness, perseverance, fairness, …). Leave blank to let the story unfold without a guiding moral.
				</small>
			</div>

			<!-- Finale reward -->
			<div style="margin-bottom: 1.5rem;">
				<label style="display: block;">
					<span style="display: block; font-weight: 600; margin-bottom: 0.25rem;">Finale reward <em style="font-weight: normal; color: #666;">(optional)</em></span>
					<input
						type="text"
						bind:value={finaleReward}
						maxlength="200"
						placeholder="lördagsgodis"
						style="width: 100%; max-width: 500px; padding: 0.4rem 0.5rem;"
					/>
				</label>
				<small style="display: block; color: #666; margin-top: 0.25rem; max-width: 500px;">
					A treat of your choice to be rewarded for completing the adventure. Leave blank if you'd rather skip the reward step.
				</small>
			</div>

			<!-- Days -->
			<h2 style="margin-top: 2rem;">Daily real world challenges</h2>
			<p style="color: #666; font-size: 0.9rem; margin-top: 0;">
				Each day has 1–3 challenges. Difficulty sets the default point value, but feel free to adjust the value where needed.
			</p>

			{#each days as day, dayIndex (day.dayNumber)}
				<section style="border: 1px solid #ddd; border-radius: 6px; padding: 0.75rem 1rem; margin-bottom: 0.75rem;">
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

							<div style="display: flex; gap: 0.5rem;">
								{#each ['Easy', 'Medium', 'Hard'] as const as diff (diff)}
									<label style="font-size: 0.85rem;">
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
								onclick={() => removeTask(dayIndex, taskIndex)}
								disabled={day.tasks.length <= 1}
								title={day.tasks.length <= 1 ? 'Each day must have at least one challenge' : 'Remove challenge'}
								style="background: none; border: 1px solid #ccc; padding: 0.2rem 0.5rem; cursor: pointer;"
							>
								✕
							</button>
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
					style="padding: 0.5rem 1rem; font-weight: 600;"
				>
					{submitState.status === 'submitting' ? 'Creating…' : 'Create adventure'}
				</button>
			</div>
		</form>

		{#if submitState.status === 'error'}
			<div style="color: crimson; margin-top: 1rem;">
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