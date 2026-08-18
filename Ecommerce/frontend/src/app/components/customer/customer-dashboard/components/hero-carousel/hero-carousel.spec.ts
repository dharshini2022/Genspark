import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HeroCarouselComponent } from './hero-carousel';
import { Router } from '@angular/router';

describe('HeroCarouselComponent', () => {
  let component: HeroCarouselComponent;
  let fixture: ComponentFixture<HeroCarouselComponent>;
  let mockRouter: any;

  beforeEach(async () => {
    mockRouter = {
      navigate: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [HeroCarouselComponent],
      providers: [
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HeroCarouselComponent);
    component = fixture.componentInstance;
    
    vi.useFakeTimers();
  });

  afterEach(() => {
    fixture.destroy();
    vi.useRealTimers();
  });

  it('should create and start auto play', () => {
    fixture.detectChanges(); // triggers ngOnInit
    expect(component).toBeTruthy();
    expect(component.activeIdx()).toBe(0);

    // Advance time by 5000ms
    vi.advanceTimersByTime(5000);
    expect(component.activeIdx()).toBe(1);

    vi.advanceTimersByTime(5000);
    expect(component.activeIdx()).toBe(2);

    // Wrap around
    vi.advanceTimersByTime(5000);
    expect(component.activeIdx()).toBe(0);
  });

  it('should stop auto play on destroy', () => {
    fixture.detectChanges();
    fixture.destroy();
    // After destroy, timer advance should not change index
    const lastIdx = component.activeIdx();
    vi.advanceTimersByTime(5000);
    expect(component.activeIdx()).toBe(lastIdx);
  });

  it('should set slide manually and reset auto play timer', () => {
    fixture.detectChanges();
    expect(component.activeIdx()).toBe(0);

    component.setSlide(2);
    expect(component.activeIdx()).toBe(2);

    // It should start autoplay again, meaning 5000ms later it updates to 0 (since length is 3)
    vi.advanceTimersByTime(5000);
    expect(component.activeIdx()).toBe(0);
  });

  it('should navigate on action click', () => {
    fixture.detectChanges();
    const query = { sortBy: 'discount' };
    component.onActionClick(query);
    expect(mockRouter.navigate).toHaveBeenCalledWith(['/customer-home/products-list'], {
      queryParams: query
    });
  });

  describe('HTML Template rendering and actions', () => {
    it('should trigger setSlide on indicator dot click', () => {
      fixture.detectChanges();
      const dots = fixture.debugElement.nativeElement.querySelectorAll('.indicator-dot');
      expect(dots.length).toBe(3);

      const spy = vi.spyOn(component, 'setSlide');
      dots[1].click();
      expect(spy).toHaveBeenCalledWith(1);
    });

    it('should stop autoplay on mouseenter and resume on mouseleave', () => {
      fixture.detectChanges();
      const container = fixture.debugElement.nativeElement.querySelector('.carousel-container');
      
      const spyStop = vi.spyOn(component, 'stopAutoPlay');
      const spyStart = vi.spyOn(component, 'startAutoPlay');

      container.dispatchEvent(new MouseEvent('mouseenter'));
      expect(spyStop).toHaveBeenCalled();

      container.dispatchEvent(new MouseEvent('mouseleave'));
      expect(spyStart).toHaveBeenCalled();
    });
  });
});
